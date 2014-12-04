using NHibernate.Id;

namespace FluentNHibernate.Mapping
{
    public static class FluentNHibernateExtensions
    {
        public static IdentityPart Block(this IdentityGenerationStrategyBuilder<IdentityPart> strategyBuilder, string blockSize)
        {
            return strategyBuilder.Custom<TableBlockGenerator>(paramBuilder => paramBuilder.AddParam("block_size", blockSize));
        }

        public static IdentityPart Block(this IdentityGenerationStrategyBuilder<IdentityPart> strategyBuilder, string table, string column, string blockSize)
        {
            return strategyBuilder.Custom<TableBlockGenerator>(paramBuilder =>
            {
                paramBuilder.AddParam("table", table);
                paramBuilder.AddParam("column", column);
                paramBuilder.AddParam("block_size", blockSize);
            });
        }

        public static IdentityPart Block(this IdentityGenerationStrategyBuilder<IdentityPart> strategyBuilder, string table, string column, string blockSize, string where)
        {
            return strategyBuilder.Custom<TableBlockGenerator>(paramBuilder =>
            {
                paramBuilder.AddParam("table", table);
                paramBuilder.AddParam("column", column);
                paramBuilder.AddParam("block_size", blockSize);
                paramBuilder.AddParam("where", where);
            });
        }
    }
}
