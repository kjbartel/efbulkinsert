using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Data.SqlClient;
using EntityFramework.BulkInsert.Extensions;

namespace EntityFramework.BulkInsert.Providers
{
    public interface IEfBulkInsertProvider
    {
        IDbConnection GetConnection();
        void Run<T>(IEnumerable<T> entities, BulkInsertOptions options);
        void Run<T>(IEnumerable<T> entities, IDbTransaction transaction, BulkInsertOptions options);
        IEfBulkInsertProvider SetContext(DbContext context);
    }
}