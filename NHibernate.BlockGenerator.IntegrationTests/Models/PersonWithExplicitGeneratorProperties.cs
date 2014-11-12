
namespace NHibernate.BlockGenerator.IntegrationTests.Models
{
    /// <summary>
    /// This is mapped using explicit generator properties.
    /// </summary>
    class PersonWithExplicitGeneratorProperties
    {
        public virtual long Id { get; set; }
        public virtual string FirstName { get; set; }
        public virtual string LastName { get; set; }
    }
}
