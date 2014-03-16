using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using EntityFramework.BulkInsert.Extensions;
using EntityFramework.BulkInsert.Helpers;
using EntityFramework.MappingAPI;
using EntityFramework.MappingAPI.Extensions;

namespace EntityFramework.BulkInsert.Providers
{
    public class EfSqlBulkInsertProviderWithMappedDataReader : ProviderBase<SqlConnection, SqlTransaction>
    {
        /*
        public override void Run<T>(IEnumerable<T> entities, BulkInsertOptions options)
        {
            var baseType = typeof(T);
            var allTypes = baseType.GetDerivedTypes(true);

            var neededMappings = allTypes.ToDictionary(x => x, x => Context.Db(x));

            var keepIdentity = (SqlBulkCopyOptions.KeepIdentity & options) > 0;
            using (var reader = new MappedDataReader<T>(entities, neededMappings, keepIdentity))
            {
                using (var sqlBulkCopy = new SqlBulkCopy(transaction.Connection, options, transaction))
                {
                    sqlBulkCopy.BatchSize = batchSize;
                    sqlBulkCopy.DestinationTableName = string.Format("[{0}].[{1}]", reader.SchemaName, reader.TableName);
                    //sqlBulkCopy.DestinationTableName = reader.TableName;
#if !NET40
                    //sqlBulkCopy.EnableStreaming = true;
#endif

                    foreach (var kvp in reader.Mappings)
                    {
                        sqlBulkCopy.ColumnMappings.Add(kvp.Key, kvp.Value);
                    }

                    sqlBulkCopy.WriteToServer(reader);
                }
            }
        }
        */

        public override void Run<T>(IEnumerable<T> entities, SqlTransaction transaction, BulkInsertOptions options)
        {
            var keepIdentity = (SqlBulkCopyOptions.KeepIdentity & options.SqlBulkCopyOptions) > 0;
            using (var reader = new MappedDataReader<T>(entities, Context))
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

        protected override SqlConnection CreateConnection()
        {
            return new SqlConnection(ConnectionString);
        }
    }
}