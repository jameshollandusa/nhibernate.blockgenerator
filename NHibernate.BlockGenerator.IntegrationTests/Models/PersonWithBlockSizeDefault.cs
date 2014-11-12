
namespace NHibernate.BlockGenerator.IntegrationTests.Models
{
    /// <summary>
    /// Person is mapped using default generator properties.
    /// </summary>
    class PersonWithBlockSizeDefault
    {
        public virtual long Id { get; set; }
        public virtual string FirstName { get; set; }
        public virtual string LastName { get; set; }
    }
}
