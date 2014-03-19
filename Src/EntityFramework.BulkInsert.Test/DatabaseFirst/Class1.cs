using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EntityFramework.BulkInsert.Extensions;
using EntityFramework.BulkInsert.Test.CodeFirst;
using NUnit.Framework;

namespace EntityFramework.BulkInsert.Test.DatabaseFirst
{
#if EF6
    [DbConfigurationType(typeof(TestContextConfig))]
#endif
    public partial class DbFirstTestEntities
    {
        
    }

    [TestFixture]
    public class Test
    {
        public DbFirstTestEntities GetContext()
        {
            return new DbFirstTestEntities();
        }

        [Test]
        public void Insert()
        {
            using (var context = GetContext())
            {
                var now = DateTime.Now;
                var g = Guid.NewGuid().ToString("N");
                var books = new[]
                {
                    new Books
                    {
                        CreatedAt = now,
                        ISBN10 = "sfsfasdfsd",
                        ISBN13 = "asdfasfd",
                        Title = "Foo",
                        ModifiedAt = now,
                        Edition = g
                    }
                };

                context.BulkInsert(books);

                var lastBook = context.Books.OrderByDescending(x => x.CreatedAt).First();

                Assert.AreEqual(g, lastBook.Edition);
            }
        }
    }
}
