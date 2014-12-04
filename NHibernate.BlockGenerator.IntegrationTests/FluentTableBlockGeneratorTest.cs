using NHibernate.BlockGenerator.IntegrationTests.Maps;
using NHibernate.BlockGenerator.IntegrationTests.Models;
using NHibernate.Mapping;
using Xunit;

namespace NHibernate.BlockGenerator.IntegrationTests
{
    public class FluentTableBlockGeneratorTest
    {
        [Fact]
        public void VerifyFluentConfigurationWithBlockSizeSpecified()
        {
            var configuration = new ConfigurationFactory().CreateFluentConfiguration()
                .Mappings(m => m.FluentMappings.Add<PersonWithBlockSizeTwoMap>())
                .BuildConfiguration();

            var mapping = configuration.GetClassMapping(typeof(PersonWithBlockSizeTwo));
            var identifierInfo = (SimpleValue) mapping.Identifier;

            Assert.True(
                identifierInfo.IdentifierGeneratorStrategy.StartsWith("NHibernate.Id.TableBlockGenerator"));

            Assert.Equal(1, identifierInfo.IdentifierGeneratorProperties.Count);
            Assert.Equal("2", identifierInfo.IdentifierGeneratorProperties["block_size"]);
        }

        [Fact]
        public void VerifyFluentConfigurationWithBlockSizeAndTableAndColumnNamesSpecified()
        {
            var configuration = new ConfigurationFactory().CreateFluentConfiguration()
                .Mappings(m => m.FluentMappings.Add<PersonWithBlockSizeTwoAndCustomTableAndColumnNamesMap>())
                .BuildConfiguration();

            var mapping = configuration.GetClassMapping(typeof(PersonWithBlockSizeTwo));
            var identifierInfo = (SimpleValue)mapping.Identifier;

            Assert.True(
                identifierInfo.IdentifierGeneratorStrategy.StartsWith("NHibernate.Id.TableBlockGenerator"));

            Assert.Equal(3, identifierInfo.IdentifierGeneratorProperties.Count);
            Assert.Equal("2", identifierInfo.IdentifierGeneratorProperties["block_size"]);
            Assert.Equal("HibernateUniqueKey", identifierInfo.IdentifierGeneratorProperties["table"]);
            Assert.Equal("NextId", identifierInfo.IdentifierGeneratorProperties["column"]);
        }

        [Fact]
        public void VerifyFluentConfigurationWithAllOptionsSpecified()
        {
            var configuration = new ConfigurationFactory().CreateFluentConfiguration()
                .Mappings(m => m.FluentMappings.Add<PersonWithBlockSizeTwoAndAllCustomOptionsMap>())
                .BuildConfiguration();

            var mapping = configuration.GetClassMapping(typeof(PersonWithBlockSizeTwo));
            var identifierInfo = (SimpleValue)mapping.Identifier;

            Assert.True(
                identifierInfo.IdentifierGeneratorStrategy.StartsWith("NHibernate.Id.TableBlockGenerator"));

            Assert.Equal(4, identifierInfo.IdentifierGeneratorProperties.Count);
            Assert.Equal("2", identifierInfo.IdentifierGeneratorProperties["block_size"]);
            Assert.Equal("HibernateUniqueKey", identifierInfo.IdentifierGeneratorProperties["table"]);
            Assert.Equal("NextId", identifierInfo.IdentifierGeneratorProperties["column"]);
            Assert.Equal("where_string", identifierInfo.IdentifierGeneratorProperties["where"]);
        }

        [Fact]
        public void VerifyFluentConfigurationCanBuildSessionFactory()
        {
            new ConfigurationFactory().CreateFluentConfiguration()
                .Mappings(m => m.FluentMappings.Add<PersonWithBlockSizeTwoMap>())
                .BuildSessionFactory();
        }
    }
}
