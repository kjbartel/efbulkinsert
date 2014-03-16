using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Data.SqlServerCe;
using System.Linq;
using EntityFramework.BulkInsert.Extensions;
using EntityFramework.BulkInsert.Helpers;
using EntityFramework.BulkInsert.Providers;

namespace EntityFramework.BulkInsert.SqlServerCe
{
    public class SqlCeBulkInsertProvider : ProviderBase<SqlCeConnection, SqlCeTransaction>
    {
        protected override SqlCeConnection CreateConnection()
        {
            return new SqlCeConnection(ConnectionString);
        }

        protected override string ConnectionString
        {
            get { return Context.Database.Connection.ConnectionString; }
        }

        public override void Run<T>(IEnumerable<T> entities, SqlCeTransaction transaction, BulkInsertOptions options)
        {
            var keepIdentity = (SqlBulkCopyOptions.KeepIdentity & options.SqlBulkCopyOptions) > 0;
            var keepNulls = (SqlBulkCopyOptions.KeepNulls & options.SqlBulkCopyOptions) > 0;
            
            using (var reader = new MappedDataReader<T>(entities, Context))
            {
                if (keepIdentity)
                {
                    SetIdentityInsert(transaction, reader.TableName, true);
                }

                var sqlCeConnection = (SqlCeConnection) transaction.Connection;
                var colInfos = ColInfos(sqlCeConnection, reader)
                    .Values
                    .Where(x => !x.IsIdentity || keepIdentity)
                    .ToArray();

                using (var cmd = new SqlCeCommand(reader.TableName, sqlCeConnection, transaction))
                {
                    cmd.CommandType = CommandType.TableDirect;
                    using (var rs = cmd.ExecuteResultSet(ResultSetOptions.Updatable))
                    {
                        var rec = rs.CreateRecord();

                        while (reader.Read())
                        {
                            foreach (var colInfo in colInfos)
                            {
                                var value = reader.GetValue(colInfo.ReaderKey);
                                if (value == null && keepNulls)
                                {
                                    rec.SetValue(colInfo.OrdinalPosition, DBNull.Value);
                                }
                                else
                                {
                                    rec.SetValue(colInfo.OrdinalPosition, value);
                                }
                            }
                            rs.Insert(rec);
                        }
                    }
                }

                if (keepIdentity)
                {
                    SetIdentityInsert(transaction, reader.TableName, false);
                }
            }
        }

        private void SetIdentityInsert(SqlCeTransaction transaction, string tableName, bool on)
        {
            var commandText = string.Format("SET IDENTITY_INSERT [{0}] {1}", tableName, on ? "ON" : "OFF");
            using (var cmd = new SqlCeCommand(commandText, (SqlCeConnection)transaction.Connection, transaction))
            {
                cmd.ExecuteNonQuery();
            }
        }

        private class ColInfo
        {
            public int OrdinalPosition { get; set; }
            public int ReaderKey { get; set; }
            public bool IsIdentity { get; set; }
        }

        private static Dictionary<string, ColInfo> ColInfos<T>(SqlCeConnection sqlCeConnection, MappedDataReader<T> reader)
        {
            var dtColumns = sqlCeConnection.GetSchema("Columns");

            var colInfos = new Dictionary<string, ColInfo>();
            foreach (DataRow row in dtColumns.Rows)
            {
                var tableName = (string) row.ItemArray[2];
                if (tableName != reader.TableName)
                {
                    continue;
                }

                var columnName = (string) row.ItemArray[3];
                var ordinal = (int) row.ItemArray[4] - 1;

                colInfos[columnName] = new ColInfo {OrdinalPosition = ordinal};
            }

            foreach (var kvp in reader.Cols)
            {
                var colName = kvp.Value.ColumnName;
                if (colInfos.ContainsKey(colName))
                {
                    colInfos[colName].ReaderKey = kvp.Key;
                    colInfos[colName].IsIdentity = kvp.Value.IsIdentity;
                }
            }
            return colInfos;
        }
    }
}
