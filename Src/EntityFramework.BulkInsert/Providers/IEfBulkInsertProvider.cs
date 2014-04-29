using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using EntityFramework.BulkInsert.Extensions;

namespace EntityFramework.BulkInsert.Providers
{
    public interface IEfBulkInsertProvider
    {
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        IDbConnection GetConnection();

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="entities"></param>
        /// <param name="options"></param>
        void Run<T>(IEnumerable<T> entities, BulkInsertOptions options);

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="entities"></param>
        /// <param name="transaction"></param>
        /// <param name="options"></param>
        void Run<T>(IEnumerable<T> entities, IDbTransaction transaction, BulkInsertOptions options);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        IEfBulkInsertProvider SetContext(DbContext context);

        /// <summary>
        /// Get sql grography object from well known text
        /// </summary>
        /// <param name="wkt">Well known text representation of the value</param>
        /// <param name="srid">The identifier associated with the coordinate system.</param>
        /// <returns></returns>
        object GetSqlGeography(string wkt, int srid);
      
        /// <summary>
        /// Current DbContext
        /// </summary>
        DbContext Context { get; }
    }
}