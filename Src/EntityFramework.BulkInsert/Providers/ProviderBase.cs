using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
#if EF6
using System.Data.Entity.Spatial;
#else
using System.Data.Spatial;
#endif
using System.Data.SqlClient;
using EntityFramework.BulkInsert.Extensions;

namespace EntityFramework.BulkInsert.Providers
{
    public abstract class ProviderBase<TConnection, TTransaction> : IEfBulkInsertProvider 
        where TConnection : IDbConnection
        where TTransaction : IDbTransaction
    {
        /// <summary>
        /// Current DbContext
        /// </summary>
        public DbContext Context { get; private set; }

        /// <summary>
        /// Connection string which current dbcontext is using
        /// </summary>
        protected virtual string ConnectionString
        {
            get
            {
                return (string)Context.Database.Connection.GetPrivateFieldValue("_connectionString");
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="dbGeography"></param>
        /// <returns></returns>
        public abstract object ConvertDbGeography(DbGeography dbGeography);

        /// <summary>
        /// Sets DbContext for bulk insert to use
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        public IEfBulkInsertProvider SetContext(DbContext context)
        {
            Context = context;
            return this;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public IDbConnection GetConnection()
        {
            return CreateConnection();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        protected abstract TConnection CreateConnection();

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="entities"></param>
        /// <param name="transaction"></param>
        /// <param name="options"></param>
        public void Run<T>(IEnumerable<T> entities, IDbTransaction transaction, BulkInsertOptions options)
        {
            Run(entities, (TTransaction)transaction, options);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="entities"></param>
        /// <param name="options"></param>
        public virtual void Run<T>(IEnumerable<T> entities, BulkInsertOptions options)
        {
            using (var dbConnection = GetConnection())
            {
                dbConnection.Open();

                using (var transaction = dbConnection.BeginTransaction())
                {
                    try
                    {
                        Run(entities, transaction, options);
                        transaction.Commit();
                    }
                    catch (Exception)
                    {
                        if (transaction.Connection != null)
                        {
                            transaction.Rollback();
                        }
                        throw;
                    }
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="entities"></param>
        /// <param name="transaction"></param>
        /// <param name="options"></param>
        public abstract void Run<T>(IEnumerable<T> entities, TTransaction transaction, BulkInsertOptions options);
    }
}