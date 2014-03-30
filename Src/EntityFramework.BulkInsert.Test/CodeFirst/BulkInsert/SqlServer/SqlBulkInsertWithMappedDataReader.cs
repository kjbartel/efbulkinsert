using EntityFramework.BulkInsert.Providers;

namespace EntityFramework.BulkInsert.Test.CodeFirst.BulkInsert.SqlServer
{
    public class SqlBulkInsertWithMappedDataReader : BulkInsertTestBase<EfSqlBulkInsertProviderWithMappedDataReader, TestContext>
    {
        protected override string ProviderConnectionType
        {
            get { return "System.Data.SqlClient.SqlConnection"; }
        }

        protected override TestContext GetContext()
        {
            return new TestContext();
        }
    }
}
