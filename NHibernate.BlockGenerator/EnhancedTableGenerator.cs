﻿using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Runtime.CompilerServices;
using NHibernate.AdoNet.Util;
using NHibernate.Engine;
using NHibernate.SqlCommand;
using NHibernate.SqlTypes;
using NHibernate.Type;
using NHibernate.Util;

namespace NHibernate.Id
{
    /// <summary>
    /// An <see cref="IIdentifierGenerator" /> that uses a database table to store the last
    /// generated value, but gives the option to control the generated value to derived
    /// classes. This is simply a clone of <see cref="TableGenerator" /> that exposes
    /// an additional extension point.
    /// </summary>
    public partial class EnhancedTableGenerator : TransactionHelper, IPersistentIdentifierGenerator, IConfigurable
	{
        private static readonly INHibernateLogger log = NHibernateLogger.For(typeof(EnhancedTableGenerator));

        /// <summary>
        /// An additional where clause that is added to 
        /// the queries against the table.
        /// </summary>
        public const string Where = "where";

		/// <summary>
		/// The name of the column parameter.
		/// </summary>
		public const string ColumnParamName = "column";

		/// <summary>
		/// The name of the table parameter.
		/// </summary>
		public const string TableParamName = "table";

		/// <summary>Default column name </summary>
		public const string DefaultColumnName = "next_id";

		/// <summary>Default table name </summary>
		public const string DefaultTableName = "hibernate_unique_key";

		private string tableName;
		private string columnName;
		private string whereClause;
		private string query;

		protected SqlType columnSqlType;
		protected PrimitiveType columnType;

		private SqlString updateSql;
		private SqlType[] parameterTypes;

		#region IConfigurable Members

		/// <summary>
		/// Configures the EnhancedTableGenerator by reading the value of <c>table</c>, 
		/// <c>column</c>, and <c>schema</c> from the <c>parms</c> parameter.
		/// </summary>
		/// <param name="type">The <see cref="IType"/> the identifier should be.</param>
		/// <param name="parms">An <see cref="IDictionary"/> of Param values that are keyed by parameter name.</param>
        /// <param name="dialect">The <see cref="Dialect.Dialect"/> to help with Configuration.</param>
        public virtual void Configure(IType type, IDictionary<string, string> parms, Dialect.Dialect dialect)
		{
			tableName = PropertiesHelper.GetString(TableParamName, parms, DefaultTableName);
			columnName = PropertiesHelper.GetString(ColumnParamName, parms, DefaultColumnName);
			whereClause = PropertiesHelper.GetString(Where, parms, "");
			string schemaName = PropertiesHelper.GetString(PersistentIdGeneratorParmsNames.Schema, parms, null);
			string catalogName = PropertiesHelper.GetString(PersistentIdGeneratorParmsNames.Catalog, parms, null);

			if (tableName.IndexOf('.') < 0)
			{
				tableName = dialect.Qualify(catalogName, schemaName, tableName);
			}

			var selectBuilder = new SqlStringBuilder(100);
			selectBuilder.Add("select " + columnName)
				.Add(" from " + dialect.AppendLockHint(LockMode.Upgrade, tableName));
			if (string.IsNullOrEmpty(whereClause) == false)
			{
				selectBuilder.Add(" where ").Add(whereClause);
			}
			selectBuilder.Add(dialect.ForUpdateString);

			query = selectBuilder.ToString();

			columnType = type as PrimitiveType;
			if (columnType == null)
			{
				log.Error("Column type for EnhancedTableGenerator is not a value type");
				throw new ArgumentException("type is not a ValueTypeType", "type");
			}

			// build the sql string for the Update since it uses parameters
			if (type is Int16Type)
			{
				columnSqlType = SqlTypeFactory.Int16;
			}
			else if (type is Int64Type)
			{
				columnSqlType = SqlTypeFactory.Int64;
			}
			else
			{
				columnSqlType = SqlTypeFactory.Int32;
			}

			parameterTypes = new[] {columnSqlType, columnSqlType};

			var builder = new SqlStringBuilder(100);
			builder.Add("update " + tableName + " set ")
				.Add(columnName).Add(" = ").Add(Parameter.Placeholder)
				.Add(" where ")
				.Add(columnName).Add(" = ").Add(Parameter.Placeholder);
			if (string.IsNullOrEmpty(whereClause) == false)
			{
				builder.Add(" and ").Add(whereClause);
			}

			updateSql = builder.ToSqlString();
		}

		#endregion

		#region IIdentifierGenerator Members

		/// <summary>
		/// Generate a <see cref="short"/>, <see cref="int"/>, or <see cref="long"/> 
		/// for the identifier by selecting and updating a value in a table.
		/// </summary>
		/// <param name="session">The <see cref="ISessionImplementor"/> this id is being generated in.</param>
		/// <param name="obj">The entity for which the id is being generated.</param>
		/// <returns>The new identifier as a <see cref="short"/>, <see cref="int"/>, or <see cref="long"/>.</returns>
		[MethodImpl(MethodImplOptions.Synchronized)]
		public virtual object Generate(ISessionImplementor session, object obj)
		{
			// This has to be done using a different connection to the containing
			// transaction becase the new id value must remain valid even if the
			// containing transaction rolls back.
			return DoWorkInNewTransaction(session);
		}

		#endregion

		#region IPersistentIdentifierGenerator Members

		/// <summary>
		/// The SQL required to create the database objects for a EnhancedTableGenerator.
		/// </summary>
		/// <param name="dialect">The <see cref="Dialect"/> to help with creating the sql.</param>
		/// <returns>
		/// An array of <see cref="string"/> objects that contain the Dialect specific sql to 
		/// create the necessary database objects and to create the first value as <c>1</c> 
		/// for the EnhancedTableGenerator.
		/// </returns>
		public virtual string[] SqlCreateStrings(Dialect.Dialect dialect)
		{
			// changed the first value to be "1" by default since an uninitialized Int32 is 0 - leaving
			// it at 0 would cause problems with an unsaved-value="0" which is what most people are 
			// defaulting <id>'s with Int32 types at.
			return new[]
					{
						"create table " + tableName + " ( " + columnName + " " + dialect.GetTypeName(columnSqlType) + " )",
						"insert into " + tableName + " values ( 1 )"
					};
		}

		/// <summary>
		/// The SQL required to remove the underlying database objects for a EnhancedTableGenerator.
		/// </summary>
		/// <param name="dialect">The <see cref="Dialect"/> to help with creating the sql.</param>
		/// <returns>
		/// A <see cref="string"/> that will drop the database objects for the EnhancedTableGenerator.
		/// </returns>
        public virtual string[] SqlDropString(Dialect.Dialect dialect)
		{
			return new[] {dialect.GetDropTableString(tableName)};
		}

		/// <summary>
		/// Return a key unique to the underlying database objects for a EnhancedTableGenerator.
		/// </summary>
		/// <returns>
		/// The configured table name.
		/// </returns>
		public string GeneratorKey()
		{
			return tableName;
		}

		#endregion

		public override object DoWorkInCurrentTransaction(ISessionImplementor session, DbConnection conn,
														  DbTransaction transaction)
		{
			long result;
			int rows;
			do
			{
				//the loop ensure atomicitiy of the 
				//select + update even for no transaction
				//or read committed isolation level (needed for .net?)

				var qps = conn.CreateCommand();
				DbDataReader rs = null;
				qps.CommandText = query;
				qps.CommandType = CommandType.Text;
				qps.Transaction = transaction;
				PersistentIdGeneratorParmsNames.SqlStatementLogger.LogCommand("Reading high value:", qps, FormatStyle.Basic);
				try
				{
					rs = qps.ExecuteReader();
                    if (!rs.Read())
                    {
                        var errFormat = string.IsNullOrEmpty(whereClause)
                            ? "could not read a hi value - you need to populate the table: {0}"
                            : "could not read a hi value from table '{0}' using the where clause ({1})- you need to populate the table.";
				        log.Error(errFormat, tableName, whereClause);
                        throw new IdentifierGenerationException(string.Format(errFormat, tableName, whereClause));
				    }
				    result = Convert.ToInt64(columnType.Get(rs, 0, session));
				}
				catch (Exception e)
				{
					log.Error(e, "could not read a hi value");
					throw;
				}
				finally
				{
					if (rs != null)
					{
						rs.Close();
					}
					qps.Dispose();
				}

				var ups = session.Factory.ConnectionProvider.Driver.GenerateCommand(CommandType.Text, updateSql, parameterTypes);
				ups.Connection = conn;
				ups.Transaction = transaction;

				try
				{
                    // *** 
                    // The change from the original version is the line below
                    // that makes a call to a new virtual method GetNextIdValue
                    // *** 
                    columnType.Set(ups, GetNextIdValue(result), 0, session);
					columnType.Set(ups, result, 1, session);

					PersistentIdGeneratorParmsNames.SqlStatementLogger.LogCommand("Updating id value:", ups, FormatStyle.Basic);

					rows = ups.ExecuteNonQuery();
				}
				catch (Exception e)
				{
					log.Error(e, "could not update id value in: {0}", tableName);
					throw;
				}
				finally
				{
					ups.Dispose();
				}
			}
			while (rows == 0);

			return result;
		}

        /// <summary>
        /// Returns the next id value to store in the database.
        /// Defaults to the current database value plus 1 but this behavior
        /// can be overridden in a derived class.
        /// </summary>
        /// <param name="currentIdValue">The current database id value.</param>
        /// <returns>The next id value to store in the database.</returns>
        protected virtual long GetNextIdValue(long currentIdValue)
	    {
	        return currentIdValue + 1;
	    }
	}
}