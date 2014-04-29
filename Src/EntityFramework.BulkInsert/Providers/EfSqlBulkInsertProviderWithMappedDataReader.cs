using System.Collections.Generic;

#if NET45
#if EF6
using System.Data.Entity.Spatial;
#else
using System.Data.Spatial;
#endif
#endif

using System.Data.SqlClient;
using System.Data.SqlTypes;
using System.Linq;
using EntityFramework.BulkInsert.Extensions;
using EntityFramework.BulkInsert.Helpers;
using EntityFramework.MappingAPI;
using EntityFramework.MappingAPI.Extensions;
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

#if NET45
        /// <summary>
        /// 
        /// </summary>
        /// <param name="o"></param>
        /// <returns></returns>
        public override object ConvertDbGeography(DbGeography o)
        {
            var chars = new SqlChars(o.WellKnownValue.WellKnownText);
            return SqlGeography.STGeomFromText(chars, o.CoordinateSystemId);
        }
#endif

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