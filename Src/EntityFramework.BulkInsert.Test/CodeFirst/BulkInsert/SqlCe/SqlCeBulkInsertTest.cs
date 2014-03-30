using System.Data.Entity;
using System.Data.Entity.Core.Common;
using System.Data.Entity.Infrastructure;
using System.Data.Entity.SqlServerCompact;
using EntityFramework.BulkInsert.SqlServerCe;
using EntityFramework.Bulkinsert.Test.CodeFirst;

namespace EntityFramework.BulkInsert.Test.CodeFirst.BulkInsert.SqlCe
{
    public class SqlCeBulkInsertTest : BulkInsertTestBase<SqlCeBulkInsertProvider, SqlCeContext>
    {
        private bool _loaded = false;

        public override void Setup()
        {
            if (!_loaded)
            {
                DbConfiguration.Loaded += (_, a) =>
                {
                    a.ReplaceService<DbProviderServices>((s, k) => SqlCeProviderServices.Instance);
                    a.ReplaceService<IDbConnectionFactory>(
                        (s, k) => new SqlCeConnectionFactory(SqlCeProviderServices.ProviderInvariantName));
                };
                _loaded = true;
            }

            base.Setup();
        }

        protected override SqlCeContext GetContext()
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
