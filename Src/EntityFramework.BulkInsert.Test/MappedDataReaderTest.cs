using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Diagnostics;
using EntityFramework.BulkInsert.Helpers;
using EntityFramework.BulkInsert.Test.CodeFirst;
using EntityFramework.BulkInsert.Test.Domain;
using EntityFramework.BulkInsert.Test.Domain.ComplexTypes;
using EntityFramework.MappingAPI;
using EntityFramework.MappingAPI.Extensions;
using NUnit.Framework;
using TestContext = EntityFramework.BulkInsert.Test.CodeFirst.TestContext;

namespace EntityFramework.BulkInsert.Test
{
    [TestFixture]
    public class MappedDataReaderTest : TestBase<TestContext>
    {
        [Test]
        public void Performance()
        {
            var sw = new Stopwatch();
            var swv = new Stopwatch();

            using (var ctx = new TestContext())
            {
                ctx.Database.Initialize(false);
                sw.Restart();
                using (var reader = new MappedDataReader<Page>(CreatePages(1000000), ctx))
                {
                    Console.WriteLine("Construct {0}ms", sw.Elapsed.TotalMilliseconds);
                    while (reader.Read())
                    {
                        foreach (var col in reader.Cols)
                        {
                            swv.Start();
                            var value = reader.GetValue(col.Key);
                            swv.Stop();
                        }
                    }
                }
                sw.Stop();
                Console.WriteLine("Elapsed {0}ms. Getting values took {1}ms", sw.Elapsed.TotalMilliseconds, swv.Elapsed.TotalMilliseconds);
            }
        }

        [Test]
        public void SimpleTableReader()
        {
            using (var ctx = new TestContext())
            {
                using (var reader = new MappedDataReader<Page>(new[] {new Page { Title = "test"}}, ctx))
                {
                    Assert.AreEqual(6, reader.FieldCount);
                }
            }
        }

        [Test]
        public void ComplexTypeReader()
        {
            var user = new TestUser
            {
                Contact = new Contact { Address = new Address { City = "Tallinn", Country = "Estonia"}, PhoneNumber = "1234567"},
                FirstName = "Max",
                LastName = "Lego",
                Id = Guid.NewGuid()
            };
            var emptyUser = new TestUser();

            using (var ctx = new TestContext())
            {
                using (var reader = new MappedDataReader<TestUser>(new[] { user, emptyUser }, ctx))
                {
                    Assert.AreEqual(9, reader.FieldCount);
                    
                    while (reader.Read())
                    {
                        for (int i = 0; i < reader.FieldCount; ++i)
                        {
                            Console.WriteLine("{0}: {1}", i, reader.GetValue(i));
                        }
                    }
                }
            }
        }
    }
}