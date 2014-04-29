using System.Collections.Generic;
using System.Data.SqlClient;
using System.Data.SqlTypes;
using EntityFramework.BulkInsert.Extensions;
using EntityFramework.BulkInsert.Helpers;
using Microsoft.SqlServer.Types;

namespace EntityFramework.BulkInsert.Providers
{
    public class EfSqlBulkInsertProviderWithMappedDataReader : ProviderBase<SqlConnection, SqlTransaction>
    {
        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="entities"></param>
        /// <param name="transaction"></param>
        /// <param name="options"></param>
        public override void Run<T>(IEnumerable<T> entities, SqlTransaction transaction, BulkInsertOptions options)
        {
            var keepIdentity = (SqlBulkCopyOptions.KeepIdentity & options.SqlBulkCopyOptions) > 0;
            using (var reader = new MappedDataReader<T>(entities, this))
            {
                using (var sqlBulkCopy = new SqlBulkCopy(transaction.Connection, options.SqlBulkCopyOptions, transaction))
                {
                    sqlBulkCopy.BulkCopyTimeout = options.TimeOut;
                    sqlBulkCopy.BatchSize = options.BatchSize;
                    sqlBulkCopy.DestinationTableName = string.Format("[{0}].[{1}]", reader.SchemaName, reader.TableName);
#if !NET40
                    sqlBulkCopy.EnableStreaming = options.EnableStreaming;
#endif

                    sqlBulkCopy.NotifyAfter = options.NotifyAfter;
                    if (options.Callback != null)
                    {
                        sqlBulkCopy.SqlRowsCopied += options.Callback;
                    }

                    foreach (var kvp in reader.Cols)
                    {
                        if (kvp.Value.IsIdentity && !keepIdentity)
                        {
                            continue;
                        }
                        sqlBulkCopy.ColumnMappings.Add(kvp.Value.ColumnName, kvp.Value.ColumnName);
                    }

                    sqlBulkCopy.WriteToServer(reader);
                }
            }
        }

        /// <summary>
        /// Get sql grography object from well known text
        /// </summary>
        /// <param name="wkt">Well known text representation of the value</param>
        /// <param name="srid">The identifier associated with the coordinate system.</param>
        /// <returns></returns>
        public override object GetSqlGeography(string wkt, int srid)
        {
            var chars = new SqlChars(wkt);
            return SqlGeography.STGeomFromText(chars, srid);
        }

        /// <summary>
        /// Create new sql connection
        /// </summary>
        /// <returns></returns>
        protected override SqlConnection CreateConnection()
        {
            return new SqlConnection(ConnectionString);
        }
    }
}