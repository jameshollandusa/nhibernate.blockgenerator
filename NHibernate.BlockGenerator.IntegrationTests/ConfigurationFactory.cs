using FluentNHibernate.Cfg;
using FluentNHibernate.Cfg.Db;
using NHibernate.BlockGenerator.IntegrationTests.Models;
using NHibernate.Cfg;
using NHibernate.Dialect;

namespace NHibernate.BlockGenerator.IntegrationTests
{
    class ConfigurationFactory
    {
        private const string ConnectionString = "Server=LOCALHOST;Database=BlockGeneratorTest;Integrated Security=true";

        public Configuration CreateConfiguration()
        {
            var configuration = new Configuration()
                .DataBaseIntegration(d =>
                {
                    d.ConnectionString = ConnectionString;
                    d.Dialect<MsSql2005Dialect>();
                })
                .AddClass(typeof (PersonWithExplicitGeneratorProperties))
                .AddClass(typeof (PersonWithBlockSizeDefault))
                .AddClass(typeof (PersonWithBlockSizeZero))
                .AddClass(typeof (PersonWithBlockSizeOne))
                .AddClass(typeof (PersonWithBlockSizeTwo));

            return configuration;
        }

        public FluentConfiguration CreateFluentConfiguration()
        {
            return Fluently.Configure()
                .Database(MsSqlConfiguration.MsSql2005
                    .ConnectionString(ConnectionString));
        }
    }
}
