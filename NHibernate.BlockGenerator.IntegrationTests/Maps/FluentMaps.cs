using FluentNHibernate.Mapping;
using NHibernate.BlockGenerator.IntegrationTests.Models;

namespace NHibernate.BlockGenerator.IntegrationTests.Maps
{
    class PersonWithBlockSizeTwoMap : ClassMap<PersonWithBlockSizeTwo>
    {
        public PersonWithBlockSizeTwoMap()
        {
            Id(x => x.Id).GeneratedBy.Block("2");
            Map(x => x.FirstName);
            Map(x => x.LastName);
        }
    }

    class PersonWithBlockSizeTwoAndCustomTableAndColumnNamesMap : ClassMap<PersonWithBlockSizeTwo>
    {
        public PersonWithBlockSizeTwoAndCustomTableAndColumnNamesMap()
        {
            Id(x => x.Id).GeneratedBy.Block("HibernateUniqueKey", "NextId", "2");
            Map(x => x.FirstName);
            Map(x => x.LastName);
        }
    }

    class PersonWithBlockSizeTwoAndAllCustomOptionsMap : ClassMap<PersonWithBlockSizeTwo>
    {
        public PersonWithBlockSizeTwoAndAllCustomOptionsMap()
        {
            Id(x => x.Id).GeneratedBy.Block("HibernateUniqueKey", "NextId", "2", "where_string");
            Map(x => x.FirstName);
            Map(x => x.LastName);
        }
    }
}
