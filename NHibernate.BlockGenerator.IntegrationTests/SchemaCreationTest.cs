using NHibernate.Tool.hbm2ddl;
using Xunit;

namespace NHibernate.BlockGenerator.IntegrationTests
{
    public class SchemaCreationTest
    {
        /// <summary>
        /// This test creates the schema for the rest of the tests and also exports a copy to a file.
        /// It only needs to be run once. You need to create the database yourself.
        /// </summary>
        [RunnableInDebugOnly]
        public void RecreateDatabase()
        {
            var configuration = new ConfigurationFactory().CreateConfiguration();
            var schemaExport = new SchemaExport(configuration);

            // The current dir is the bin/Debug folder so we back out of there.
            schemaExport.SetOutputFile(@"..\..\..\database\create_schema.sql");
            schemaExport.Execute(true, true, false);

            // We expect two generator tables - the default one used by most of the model classes,
            // and the one with all params specified that uses a custom table and column.

            var sessionFactory = configuration.BuildSessionFactory();

            // Check default generator table
            VerifyGeneratorTable(sessionFactory, "hibernate_unique_key", "next_id", 1);
            // Check params generator table
            VerifyGeneratorTable(sessionFactory, "HibernateUniqueKey", "NextId", 1);
        }

        private void VerifyGeneratorTable(ISessionFactory sessionFactory, string tableName, string columnName, long initialValue)
        {
            using (var session = sessionFactory.OpenSession())
            using (var tx = session.BeginTransaction())
            {
                // Check the column exists for the given table 
                var directSql = session.CreateSQLQuery("SELECT COUNT(1) AS Count FROM sys.tables t INNER JOIN sys.columns c on t.object_id = c.object_id WHERE t.name = ? and c.name = ?");
                directSql.SetParameter(0, tableName);
                directSql.SetParameter(1, columnName);
                directSql.AddScalar("Count", NHibernateUtil.Int32);
                var count = directSql.UniqueResult<int>();
                Assert.Equal(1, count);

                // Check the value of the column in the given table 
                directSql = session.CreateSQLQuery($"SELECT {columnName} FROM {tableName}");
                directSql.AddScalar(columnName, NHibernateUtil.Int64);
                var value = directSql.UniqueResult<long>();
                Assert.Equal(initialValue, value);

                tx.Commit();
            }
        }
    }
}
