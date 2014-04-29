﻿using System;
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
            get { return DbConnection.ConnectionString; }
        }

        /// <summary>
        /// Get sql grography object from well known text
        /// </summary>
        /// <param name="wkt">Well known text representation of the value</param>
        /// <param name="srid">The identifier associated with the coordinate system.</param>
        /// <returns></returns>
        public override object GetSqlGeography(string wkt, int srid)
        {
            throw new NotImplementedException();
        }

        public override void Run<T>(IEnumerable<T> entities, BulkInsertOptions options)
        {
            using (var dbConnection = GetConnection())
            {
                dbConnection.Open();

                if ((options.SqlBulkCopyOptions & SqlBulkCopyOptions.UseInternalTransaction) > 0)
                {
                    using (var transaction = dbConnection.BeginTransaction())
                    {
                        try
                        {
                            Run(entities, (SqlCeConnection)dbConnection, (SqlCeTransaction)transaction, options);
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
                else
                {
                    Run(entities, (SqlCeConnection)dbConnection, null, options);
                }
            }
        }

        private bool IsValidIdentityType(Type t)
        {
            switch (Type.GetTypeCode(t))
            {
                case TypeCode.Byte:
                case TypeCode.SByte:
                case TypeCode.UInt16:
                case TypeCode.UInt32:
                case TypeCode.UInt64:
                case TypeCode.Int16:
                case TypeCode.Int32:
                case TypeCode.Int64:
                    return true;
                default:
                    return false;
            }
        }

        private void Run<T>(IEnumerable<T> entities, SqlCeConnection connection, SqlCeTransaction transaction, BulkInsertOptions options)
        {
            bool runIdentityScripts;
            bool keepIdentity = runIdentityScripts = (SqlBulkCopyOptions.KeepIdentity & options.SqlBulkCopyOptions) > 0;
            var keepNulls = (SqlBulkCopyOptions.KeepNulls & options.SqlBulkCopyOptions) > 0;

            using (var reader = new MappedDataReader<T>(entities, this))
            {
                var identityCols = reader.Cols.Values.Where(x => x.IsIdentity).ToArray();
                if (identityCols.Length != 1 || !IsValidIdentityType(identityCols[0].Type))
                {
                    runIdentityScripts = false;
                }

                if (keepIdentity && runIdentityScripts)
                {
                    SetIdentityInsert(connection, transaction, reader.TableName, true);
                }

                var colInfos = ColInfos(connection, reader)
                    .Values
                    .Where(x => !x.IsIdentity || keepIdentity)
                    .ToArray();

                using (var cmd = CreateCommand(reader.TableName, connection, transaction))
                {
                    cmd.CommandType = CommandType.TableDirect;
                    using (var rs = cmd.ExecuteResultSet(ResultSetOptions.Updatable))
                    {
                        var rec = rs.CreateRecord();
                        int i = 0;
                        long rowsCopied = 0;
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

                            ++i;
                            if (i == options.NotifyAfter && options.Callback != null)
                            {
                                rowsCopied += i;
                                options.Callback(this, new SqlRowsCopiedEventArgs(rowsCopied));
                                i = 0;
                            }
                        }
                    }
                }

                if (keepIdentity && runIdentityScripts)
                {
                    SetIdentityInsert(connection, transaction, reader.TableName, false);
                }
            }
        }

        public override void Run<T>(IEnumerable<T> entities, SqlCeTransaction transaction, BulkInsertOptions options)
        {
            Run(entities, (SqlCeConnection)transaction.Connection, transaction, options);
        }


        private void SetIdentityInsert(SqlCeConnection connection, SqlCeTransaction transaction, string tableName, bool on)
        {
            var commandText = string.Format("SET IDENTITY_INSERT [{0}] {1}", tableName, on ? "ON" : "OFF");
            using (var cmd = CreateCommand(commandText, connection, transaction))
            {
                cmd.ExecuteNonQuery();
            }
        }

        private SqlCeCommand CreateCommand(string commandText, SqlCeConnection connection, SqlCeTransaction transaction)
        {
            var cmd = new SqlCeCommand(commandText, connection);
            if (transaction != null)
            {
                cmd.Transaction = transaction;
            }
            return cmd;

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
