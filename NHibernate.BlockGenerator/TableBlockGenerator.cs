using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using NHibernate.Engine;
using NHibernate.Type;
using NHibernate.Util;

namespace NHibernate.Id
{
    /// <summary>
    /// An <see cref="IIdentifierGenerator" /> that returns an <c>Int64</c>, constructed using
    /// a block allocation algorithm that stores the next available id value in the database.
    /// </summary>
    /// <remarks>
    /// <p>
    ///	This id generation strategy is specified in the mapping file as 
    ///	<code>
    ///	&lt;generator class="block"&gt;
    ///		&lt;param name="table"&gt;table&lt;/param&gt;
    ///		&lt;param name="column"&gt;id_column&lt;/param&gt;
    ///		&lt;param name="block_size"&gt;block_size&lt;/param&gt;
    ///		&lt;param name="schema"&gt;db_schema&lt;/param&gt;
    ///	&lt;/generator&gt;
    ///	</code>
    /// </p>
    /// <p>
    /// The <c>table</c> and <c>column</c> parameters are required, the <c>block_size</c> and 
    /// <c>schema</c> are optional.
    /// </p>
    /// <p>
    /// The id value MUST be fetched in a separate transaction to the <c>ISession</c>
    /// transaction so the generator must be able to obtain a new connection and 
    /// commit it. Hence this implementation may not be used when the user is supplying
    /// connections.
    /// </p>
    /// </remarks>
    public partial class TableBlockGenerator : EnhancedTableGenerator
	{
        private static readonly INHibernateLogger log = NHibernateLogger.For(typeof(TableBlockGenerator));

		/// <summary>
		/// The name of the block size parameter.
		/// </summary>
		public const string BlockSize = "block_size";

		private long hi;
		private long lo;
		private long blockSize;
		private System.Type returnClass;

        #region IConfigurable Members

        /// <summary>
        /// Configures the TableBlockGenerator by reading the value of <c>table</c>, 
        /// <c>column</c>, <c>block_size</c>, and <c>schema</c> from the <c>parms</c> parameter.
        /// </summary>
        /// <param name="type">The <see cref="IType"/> the identifier should be.</param>
        /// <param name="parms">An <see cref="IDictionary"/> of Param values that are keyed by parameter name.</param>
        /// <param name="dialect">The <see cref="Dialect.Dialect"/> to help with Configuration.</param>
        public override void Configure(IType type, IDictionary<string, string> parms, Dialect.Dialect dialect)
		{
			base.Configure(type, parms, dialect);
			blockSize = PropertiesHelper.GetInt64(BlockSize, parms, Int16.MaxValue);
			lo = blockSize + 1; // so we "clock over" on the first invocation
			returnClass = type.ReturnedClass;
		}

		#endregion

		#region IIdentifierGenerator Members

		/// <summary>
		/// Generate a <see cref="Int64"/> for the identifier by selecting and updating a value in a table.
		/// </summary>
		/// <param name="session">The <see cref="ISessionImplementor"/> this id is being generated in.</param>
		/// <param name="obj">The entity for which the id is being generated.</param>
		/// <returns>The new identifier as a <see cref="Int64"/>.</returns>
		[MethodImpl(MethodImplOptions.Synchronized)]
		public override object Generate(ISessionImplementor session, object obj)
		{
            // Note these checks are subtlely different from the similar code in TableHiLoGenerator but
            // this is by design and appropriate for the block generator algorithm.
			if (blockSize < 2)
			{
                // Behave as if block size is 1, and don't allow an ID of 0.
				//keep the behavior consistent even for boundary usages
				long val = Convert.ToInt64(base.Generate(session, obj));
				if (val == 0)
					val = Convert.ToInt64(base.Generate(session, obj));
				return IdentifierGeneratorFactory.CreateNumber(val, returnClass);
			}

			if (lo >= blockSize)
			{
				hi = Convert.ToInt64(base.Generate(session, obj));
                // Block size is at least 2 here, so we can skip 0 and carry on.
                lo = (hi == 0) ? 1 : 0;
				log.Debug("New id value: {0}", hi);
			}

			return IdentifierGeneratorFactory.CreateNumber(hi + lo++, returnClass);
		}

		#endregion

        /// <summary>
        /// Returns the next id value to store in the database. This is the current database value 
        /// plus the block size.
        /// </summary>
        /// <param name="currentIdValue">The current database id value.</param>
        /// <returns>The next id value to store in the database.</returns>
        protected override long GetNextIdValue(long currentIdValue)
	    {
            // Boundary case behaves as if block size is 1
            if (blockSize < 1) return currentIdValue + 1;

            return currentIdValue + blockSize;
	    }
	}
}
