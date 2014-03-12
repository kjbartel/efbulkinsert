using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Data.SqlClient;
using EntityFramework.BulkInsert.Extensions;

namespace EntityFramework.BulkInsert.Providers
{
    public abstract class ProviderBase<TConnection, TTransaction> : IEfBulkInsertProvider 
        where TConnection : IDbConnection
        where TTransaction : IDbTransaction
    {
        protected DbContext Context;
        protected abstract string ConnectionString { get; }

        /// <summary>
        /// 
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
        public void Run<T>(IEnumerable<T> entities, BulkInsertOptions options)
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