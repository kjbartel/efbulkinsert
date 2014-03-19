﻿using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Data.SqlClient;
using System.Threading.Tasks;

namespace EntityFramework.BulkInsert.Extensions
{
    public static class BulkInsertExtension
    {
        /*
        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="context"></param>
        /// <param name="entities"></param>
        /// <param name="batchSize"></param>
        /// <returns></returns>
        public static Task BulkInsertAsync<T>(this DbContext context, IEnumerable<T> entities, int? batchSize = null)
        {
            return Task.Factory.StartNew(() => context.BulkInsert(entities, SqlBulkCopyOptions.Default, batchSize));
        }

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="context"></param>
        /// <param name="entities"></param>
        /// <param name="options"></param>
        /// <param name="batchSize"></param>
        public static Task BulkInsertAsync<T>(this DbContext context, IEnumerable<T> entities, SqlBulkCopyOptions options, int? batchSize = null)
        {
            return Task.Factory.StartNew(() => context.BulkInsert(entities, options, batchSize));
        }

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="context"></param>
        /// <param name="entities"></param>
        /// <param name="transaction"></param>
        /// <param name="options"></param>
        /// <param name="batchSize"></param>
        public static Task BulkInsertAsync<T>(this DbContext context, IEnumerable<T> entities, IDbTransaction transaction, SqlBulkCopyOptions options = SqlBulkCopyOptions.Default, int? batchSize = null)
        {
            return Task.Factory.StartNew(() => context.BulkInsert(entities, transaction, options, batchSize));
        }
        */

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="context"></param>
        /// <param name="entities"></param>
        /// <param name="options"></param>
        public static void BulkInsert<T>(this DbContext context, IEnumerable<T> entities, BulkInsertOptions options)
        {
            var bulkInsert = ProviderFactory.Get(context);
            bulkInsert.Run(entities, options);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="context"></param>
        /// <param name="entities"></param>
        /// <param name="batchSize"></param>
        public static void BulkInsert<T>(this DbContext context, IEnumerable<T> entities, int? batchSize = null)
        {
            context.BulkInsert(entities, SqlBulkCopyOptions.Default, batchSize);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="context"></param>
        /// <param name="entities"></param>
        /// <param name="sqlBulkCopyOptions"></param>
        /// <param name="batchSize"></param>
        public static void BulkInsert<T>(this DbContext context, IEnumerable<T> entities, SqlBulkCopyOptions sqlBulkCopyOptions, int? batchSize = null)
        {
            var bulkInsert = ProviderFactory.Get(context);
            var options = new BulkInsertOptions {SqlBulkCopyOptions = sqlBulkCopyOptions};
            if (batchSize.HasValue)
            {
                options.BatchSize = batchSize.Value;
            }

            bulkInsert.Run(entities, options);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="context"></param>
        /// <param name="entities"></param>
        /// <param name="transaction"></param>
        /// <param name="sqlBulkCopyOptions"></param>
        /// <param name="batchSize"></param>
        public static void BulkInsert<T>(this DbContext context, IEnumerable<T> entities, IDbTransaction transaction, SqlBulkCopyOptions sqlBulkCopyOptions = SqlBulkCopyOptions.Default, int? batchSize = null)
        {
            var bulkInsert = ProviderFactory.Get(context);
            var options = new BulkInsertOptions {SqlBulkCopyOptions = sqlBulkCopyOptions};
            if (batchSize.HasValue)
            {
                options.BatchSize = batchSize.Value;
            }

            bulkInsert.Run(entities, transaction, options);
        }

        /*
        public static void BulkInsert<T>(this DbContext context, IEnumerable<T> entities,
            Func<BulkInsertOptions, BulkInsertOptions> options)
        {
            var bulkInsert = ProviderFactory.Get(context);
            bulkInsert.Run(entities, options(new BulkInsertOptions()));
        }
         * */
    }

    public static class BulkInsertDefaults
    {
        public static int BatchSize = 5000;
        public static SqlBulkCopyOptions SqlBulkCopyOptions = SqlBulkCopyOptions.Default;
        public static int TimeOut = 30;
        public static int NotifyAfter = 1000;
    }

    public class BulkInsertOptions
    {
        public int BatchSize { get; set; }
        public SqlBulkCopyOptions SqlBulkCopyOptions { get; set; }
        public int TimeOut { get; set; }
        public SqlRowsCopiedEventHandler Callback { get; set; }
        public int NotifyAfter { get; set; }

#if !NET40
        public bool EnableStreaming { get; set; }
#endif

        public BulkInsertOptions()
        {
            BatchSize = BulkInsertDefaults.BatchSize;
            SqlBulkCopyOptions = BulkInsertDefaults.SqlBulkCopyOptions;
            TimeOut = BulkInsertDefaults.TimeOut;
            NotifyAfter = BulkInsertDefaults.NotifyAfter;
        }
        /*

        /// <summary>
        /// Sets batch size
        /// </summary>
        /// <param name="batchSize"></param>
        /// <returns></returns>
        public BulkInsertOptions BatchSize(int batchSize)
        {
            BatchSizeValue = batchSize;
            return this;
        }

        /// <summary>
        /// Sets sql bulk copy timeout in seconds
        /// </summary>
        /// <param name="timeout"></param>
        /// <returns></returns>
        public BulkInsertOptions TimeOut(int timeout)
        {
            TimeOutValue = timeout;
            return this;
        }

        /// <summary>
        /// Sets SqlBulkCopy options
        /// </summary>
        /// <param name="sqlBulkCopyOptions"></param>
        /// <returns></returns>
        public BulkInsertOptions SqlBulkCopyOptions(SqlBulkCopyOptions sqlBulkCopyOptions)
        {
            SqlBulkCopyOptionsValue = sqlBulkCopyOptions;
            return this;
        }

        /// <summary>
        /// Sets callback method for sql bulk insert
        /// </summary>
        /// <param name="callback"></param>
        /// <param name="interval">Numbers of rows processed before callback is invoked</param>
        /// <returns></returns>
        public BulkInsertOptions Callback(SqlRowsCopiedEventHandler callback, int interval)
        {
            CallbackMethod = callback;
            NotifyAfterValue = interval;

            return this;
        }

#if !NET40
        /// <summary>
        /// Sets batch size
        /// </summary>
        /// <param name="enableStreaming"></param>
        /// <returns></returns>
        public BulkInsertOptions EnableStreaming(bool enableStreaming)
        {
            EnableStreamingValue = enableStreaming;
            return this;
        }
#endif
        */
    }
}
