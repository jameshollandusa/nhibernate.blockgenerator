
namespace NHibernate.BlockGenerator.IntegrationTests.Models
{
    /// <summary>
    /// This is mapped with a block size of 1 using the same generator as Person.
    /// </summary>
    class PersonWithBlockSizeOne
    {
        public virtual long Id { get; set; }
        public virtual string FirstName { get; set; }
        public virtual string LastName { get; set; }
    }
}
