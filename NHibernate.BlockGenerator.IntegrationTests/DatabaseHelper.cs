using System.Text;

namespace NHibernate.BlockGenerator.IntegrationTests
{
    static class DatabaseHelper
    {
        /// <summary>
        /// Returns an SQL script to reset the database.
        /// </summary>
        public static string ResetDatabaseScript()
        {
            var sqlStrings = new StringBuilder();
            sqlStrings.AppendLine("DELETE FROM PersonWithExplicitGeneratorProperties;");
            sqlStrings.AppendLine("DELETE FROM PersonWithBlockSizeDefault;");
            sqlStrings.AppendLine("DELETE FROM PersonWithBlockSizeZero;");
            sqlStrings.AppendLine("DELETE FROM PersonWithBlockSizeOne;");
            sqlStrings.AppendLine("DELETE FROM PersonWithBlockSizeTwo;");
            sqlStrings.AppendLine("UPDATE hibernate_unique_key SET next_id = 1;");
            sqlStrings.AppendLine("UPDATE HibernateUniqueKey SET NextId = 1;");
            return sqlStrings.ToString();
        }
    }
}
