using EntityFramework.BulkInsert.SqlServerCe;
using EntityFramework.BulkInsert.Test.CustomProvider;

namespace EntityFramework.BulkInsert.Test.CodeFirst.BulkInsert.SqlCe
{
    public class SqlCeBulkInsertTest : BulkInsertTestBase<SqlCeBulkInsertProvider, SqlCeContext>
    {
        protected override SqlCeContext GetContext(string contextName = null)
        {
            var context = new SqlCeContext("SqlCeContext");
            context.Database.CreateIfNotExists();
            return context;
        }

        public override void BulkInsertTableWithComputedColumns()
        {
            // not supported
        }

        protected override string ProviderConnectionType
        {
            get { return "System.Data.SqlServerCe.SqlCeConnection"; }
        }
    }
}
