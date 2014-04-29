using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using EntityFramework.BulkInsert.Extensions;

#if NET45
#if EF6
using System.Data.Entity.Spatial;
#else
using System.Data.Spatial;
#endif
#endif

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

#if NET45
        /// <summary>
        /// 
        /// </summary>
        /// <param name="dbGeography"></param>
        /// <returns></returns>
        object ConvertDbGeography(DbGeography dbGeography);
#endif

        /// <summary>
        /// Current DbContext
        /// </summary>
        DbContext Context { get; }
    }
}