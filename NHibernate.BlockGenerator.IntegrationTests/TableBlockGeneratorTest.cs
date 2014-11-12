using System;
using NHibernate.BlockGenerator.IntegrationTests.Models;
using Xunit;

namespace NHibernate.BlockGenerator.IntegrationTests
{
    public class TableBlockGeneratorTest
    {
        [Fact]
        public void FirstCreateOfEntityInSessionFactoryAlsoIncrementsGeneratorId()
        {
            ResetDatabase();

            var person = new PersonWithBlockSizeDefault {FirstName = "Tim", LastName = "Howard"};

            var sessionFactory = new ConfigurationFactory().CreateConfiguration().BuildSessionFactory();
            using (var session = sessionFactory.OpenSession())
            using (var tx = session.BeginTransaction())
            {
                session.Save(person);
                tx.Commit();
            }

            Assert.Equal(1, person.Id);
            Assert.Equal(1 + Int16.MaxValue, GetCurrentGeneratorValue(sessionFactory));
        }

        [Fact]
        public void SubsequentCreatesInSessionFactoryDoNotIncrementGeneratorUntilBlockSizeReached()
        {
            ResetDatabase();

            var sessionFactory = new ConfigurationFactory().CreateConfiguration().BuildSessionFactory();
            const long blockSize = 2;

            // First entity save

            var person = new PersonWithBlockSizeTwo { FirstName = "Tim", LastName = "Howard" };
            SavePersonInNewSession(sessionFactory, person);

            // After first insert the generator should be incremented
            Assert.Equal(1, person.Id);
            Assert.Equal(1 + blockSize, GetCurrentGeneratorValue(sessionFactory));

            // Second entity save

            person = new PersonWithBlockSizeTwo { FirstName = "Lionel", LastName = "Messi" };
            SavePersonInNewSession(sessionFactory, person);

            // After second insert the generator should not be incremented
            Assert.Equal(2, person.Id);
            Assert.Equal(1 + blockSize, GetCurrentGeneratorValue(sessionFactory));

            // Third entity save

            person = new PersonWithBlockSizeTwo { FirstName = "Wayne", LastName = "Rooney" };
            SavePersonInNewSession(sessionFactory, person);

            // After third insert the generator should be incremented again
            Assert.Equal(3, person.Id);
            Assert.Equal(1 + (blockSize * 2), GetCurrentGeneratorValue(sessionFactory));
        }

        [Fact]
        public void InterleavedEntitiesWithDifferentBlockSizes()
        {
            ResetDatabase();

            var sessionFactory = new ConfigurationFactory().CreateConfiguration().BuildSessionFactory();
            const long smallBlockSize = 2;
            const long defaultBlockSize = Int16.MaxValue;

            long expectedDatabaseGeneratorValue = 1;
            Assert.Equal(expectedDatabaseGeneratorValue, GetCurrentGeneratorValue(sessionFactory));

            // ** First save of entity with small block size

            var personWithSmallBlockSize = new PersonWithBlockSizeTwo { FirstName = "Tim", LastName = "Howard" };
            SavePersonInNewSession(sessionFactory, personWithSmallBlockSize);

            // Generator should be incremented by the small block size
            long expectedSmallBlockSizeGeneratorValue = expectedDatabaseGeneratorValue;
            expectedDatabaseGeneratorValue += smallBlockSize;
            Assert.Equal(expectedSmallBlockSizeGeneratorValue, personWithSmallBlockSize.Id);
            Assert.Equal(expectedDatabaseGeneratorValue, GetCurrentGeneratorValue(sessionFactory));

            // ** First save of entity with default block size

            var personWithDefaultBlockSize = new PersonWithBlockSizeDefault { FirstName = "Tim", LastName = "Howard" };
            SavePersonInNewSession(sessionFactory, personWithDefaultBlockSize);

            // This entity should be allocated a default block
            // Database Generator should be incremented by default block size
            long expectedDefaultBlockSizeGeneratorValue = expectedDatabaseGeneratorValue;
            expectedDatabaseGeneratorValue += defaultBlockSize;
            Assert.Equal(expectedDefaultBlockSizeGeneratorValue, personWithDefaultBlockSize.Id);
            Assert.Equal(expectedDatabaseGeneratorValue, GetCurrentGeneratorValue(sessionFactory));

            // ** Second save of entity with small block size

            personWithSmallBlockSize = new PersonWithBlockSizeTwo { FirstName = "Lionel", LastName = "Messi" };
            SavePersonInNewSession(sessionFactory, personWithSmallBlockSize);

            // Database Generator should not be incremented
            expectedSmallBlockSizeGeneratorValue++;
            Assert.Equal(expectedSmallBlockSizeGeneratorValue, personWithSmallBlockSize.Id);
            Assert.Equal(expectedDatabaseGeneratorValue, GetCurrentGeneratorValue(sessionFactory));

            // ** Second save of entity with default block size

            personWithDefaultBlockSize = new PersonWithBlockSizeDefault { FirstName = "Tim", LastName = "Howard" };
            SavePersonInNewSession(sessionFactory, personWithDefaultBlockSize);

            // Database Generator should not be incremented
            expectedDefaultBlockSizeGeneratorValue++;
            Assert.Equal(expectedDefaultBlockSizeGeneratorValue, personWithDefaultBlockSize.Id);
            Assert.Equal(expectedDatabaseGeneratorValue, GetCurrentGeneratorValue(sessionFactory));

            // Third save of entity with small block size

            personWithSmallBlockSize = new PersonWithBlockSizeTwo { FirstName = "Wayne", LastName = "Rooney" };
            SavePersonInNewSession(sessionFactory, personWithSmallBlockSize);

            // Database Generator should be incremented again
            expectedSmallBlockSizeGeneratorValue = expectedDatabaseGeneratorValue;
            expectedDatabaseGeneratorValue += smallBlockSize;
            Assert.Equal(expectedSmallBlockSizeGeneratorValue, personWithSmallBlockSize.Id);
            Assert.Equal(expectedDatabaseGeneratorValue, GetCurrentGeneratorValue(sessionFactory));
        }

        [Fact]
        public void GeneratorIsInitializedToZeroWithBlockSizeZero()
        {
            ResetDatabase();

            // Note: zero should be skipped as an ID to conform to expected NHibernate behavior
            // Note: a block size of less than one defaults to one and means the generator should be incremented every time.

            var sessionFactory = new ConfigurationFactory().CreateConfiguration().BuildSessionFactory();
            const long smallBlockSize = 1;

            // Set generator to zero
            using (var session = sessionFactory.OpenSession())
            using (var tx = session.BeginTransaction())
            {
                var sqlQuery = session.CreateSQLQuery("UPDATE hibernate_unique_key SET next_id = 0");
                sqlQuery.ExecuteUpdate();
                tx.Commit();
            }
            Assert.Equal(0, GetCurrentGeneratorValue(sessionFactory));

            // ** First save of entity should get ID 1 (generator incremented twice)
            var personWithSmallBlockSize = new PersonWithBlockSizeZero { FirstName = "Tim", LastName = "Howard" };
            SavePersonInNewSession(sessionFactory, personWithSmallBlockSize);
            Assert.Equal(1, personWithSmallBlockSize.Id);
            Assert.Equal(smallBlockSize * 2, GetCurrentGeneratorValue(sessionFactory));

            // ** Second save of entity should get ID 2 (generator incremented)
            personWithSmallBlockSize = new PersonWithBlockSizeZero { FirstName = "Tim", LastName = "Howard" };
            SavePersonInNewSession(sessionFactory, personWithSmallBlockSize);
            Assert.Equal(2, personWithSmallBlockSize.Id);
            Assert.Equal(smallBlockSize * 3, GetCurrentGeneratorValue(sessionFactory));

            // ** Third save of entity should get ID 3 (generator incremented)
            personWithSmallBlockSize = new PersonWithBlockSizeZero { FirstName = "Tim", LastName = "Howard" };
            SavePersonInNewSession(sessionFactory, personWithSmallBlockSize);
            Assert.Equal(3, personWithSmallBlockSize.Id);
            Assert.Equal(smallBlockSize * 4, GetCurrentGeneratorValue(sessionFactory));
        }

        [Fact]
        public void GeneratorIsInitializedToZeroWithBlockSizeOne()
        {
            ResetDatabase();

            // Note: zero should be skipped as an ID to conform to expected NHibernate behavior
            // Note: a block size of one means the generator should be incremented every time.

            var sessionFactory = new ConfigurationFactory().CreateConfiguration().BuildSessionFactory();
            const long smallBlockSize = 1;

            // Set generator to zero
            using (var session = sessionFactory.OpenSession())
            using (var tx = session.BeginTransaction())
            {
                var sqlQuery = session.CreateSQLQuery("UPDATE hibernate_unique_key SET next_id = 0");
                sqlQuery.ExecuteUpdate();
                tx.Commit();
            }
            Assert.Equal(0, GetCurrentGeneratorValue(sessionFactory));

            // ** First save of entity should get ID 1 (generator incremented twice)
            var personWithSmallBlockSize = new PersonWithBlockSizeOne { FirstName = "Tim", LastName = "Howard" };
            SavePersonInNewSession(sessionFactory, personWithSmallBlockSize);
            Assert.Equal(1, personWithSmallBlockSize.Id);
            Assert.Equal(smallBlockSize * 2, GetCurrentGeneratorValue(sessionFactory));

            // ** Second save of entity should get ID 2 (generator incremented)
            personWithSmallBlockSize = new PersonWithBlockSizeOne { FirstName = "Tim", LastName = "Howard" };
            SavePersonInNewSession(sessionFactory, personWithSmallBlockSize);
            Assert.Equal(2, personWithSmallBlockSize.Id);
            Assert.Equal(smallBlockSize * 3, GetCurrentGeneratorValue(sessionFactory));

            // ** Third save of entity should get ID 3 (generator incremented)
            personWithSmallBlockSize = new PersonWithBlockSizeOne { FirstName = "Tim", LastName = "Howard" };
            SavePersonInNewSession(sessionFactory, personWithSmallBlockSize);
            Assert.Equal(3, personWithSmallBlockSize.Id);
            Assert.Equal(smallBlockSize * 4, GetCurrentGeneratorValue(sessionFactory));

        }

        [Fact]
        public void GeneratorIsInitializedToZeroWithBlockSizeTwo()
        {
            ResetDatabase();

            // Note: zero should be skipped as an ID to conform to expected NHibernate behavior

            var sessionFactory = new ConfigurationFactory().CreateConfiguration().BuildSessionFactory();
            const long smallBlockSize = 2;

            // Set generator to zero
            using (var session = sessionFactory.OpenSession())
            using (var tx = session.BeginTransaction())
            {
                var sqlQuery = session.CreateSQLQuery("UPDATE hibernate_unique_key SET next_id = 0");
                sqlQuery.ExecuteUpdate();
                tx.Commit();
            }
            Assert.Equal(0, GetCurrentGeneratorValue(sessionFactory));

            // ** First save of entity should get ID 1 (generator incremented)
            var personWithSmallBlockSize = new PersonWithBlockSizeTwo { FirstName = "Tim", LastName = "Howard" };
            SavePersonInNewSession(sessionFactory, personWithSmallBlockSize);
            Assert.Equal(1, personWithSmallBlockSize.Id);
            Assert.Equal(smallBlockSize, GetCurrentGeneratorValue(sessionFactory));

            // ** Second save of entity should get ID 2 (generator incremented)
            personWithSmallBlockSize = new PersonWithBlockSizeTwo { FirstName = "Tim", LastName = "Howard" };
            SavePersonInNewSession(sessionFactory, personWithSmallBlockSize);
            Assert.Equal(2, personWithSmallBlockSize.Id);
            Assert.Equal(smallBlockSize * 2, GetCurrentGeneratorValue(sessionFactory));

            // ** Third save of entity should get ID 3 (generator not incremented)
            personWithSmallBlockSize = new PersonWithBlockSizeTwo { FirstName = "Tim", LastName = "Howard" };
            SavePersonInNewSession(sessionFactory, personWithSmallBlockSize);
            Assert.Equal(3, personWithSmallBlockSize.Id);
            Assert.Equal(smallBlockSize * 2, GetCurrentGeneratorValue(sessionFactory));

            // ** Fourth save of entity should get ID 4 (generator incremented)
            personWithSmallBlockSize = new PersonWithBlockSizeTwo { FirstName = "Tim", LastName = "Howard" };
            SavePersonInNewSession(sessionFactory, personWithSmallBlockSize);
            Assert.Equal(4, personWithSmallBlockSize.Id);
            Assert.Equal(smallBlockSize * 3, GetCurrentGeneratorValue(sessionFactory));
        }

        private static void SavePersonInNewSession(ISessionFactory sessionFactory, object person)
        {
            using (var session = sessionFactory.OpenSession())
            using (var tx = session.BeginTransaction())
            {
                session.Save(person);
                tx.Commit();
            }
        }

        /// <summary>
        /// Deletes all data and resets the ID generators
        /// </summary>
        private static void ResetDatabase()
        {
            var sessionFactory = new ConfigurationFactory().CreateConfiguration().BuildSessionFactory();
            using (var session = sessionFactory.OpenSession())
            using (var tx = session.BeginTransaction())
            {
                var directSql = session.CreateSQLQuery(DatabaseHelper.ResetDatabaseScript());
                directSql.ExecuteUpdate();
                tx.Commit();
            }
        }

        private static long GetCurrentGeneratorValue(ISessionFactory sessionFactory)
        {
            using (var session = sessionFactory.OpenSession())
            using (var tx = session.BeginTransaction())
            {
                var sqlQuery = session.CreateSQLQuery("SELECT next_id FROM hibernate_unique_key");
                sqlQuery.AddScalar("next_id", NHibernateUtil.Int64);
                var value = sqlQuery.UniqueResult<long>();
                tx.Commit();

                return value;
            }
        }
    }
}
