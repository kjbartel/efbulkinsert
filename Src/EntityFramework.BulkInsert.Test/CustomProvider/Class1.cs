using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Data.Entity.Migrations.Infrastructure;
using System.Data.Entity.Migrations.Model;
using System.Data.SqlServerCe;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EntityFramework.BulkInsert.Extensions;
using EntityFramework.BulkInsert.Providers;

namespace EntityFramework.BulkInsert.Test.CustomProvider
{
    public class Class1
    {
        public void Test()
        {
            ProviderFactory.Register<SqlCeBulkInsertProvider>("System.Data.SqlServerCe.SqlCeConnection");
        }
    }

    public class SqlCeBulkInsertProvider : ProviderBase<SqlCeConnection, SqlCeTransaction>
    {
        protected override SqlCeConnection CreateConnection()
        {
            return new SqlCeConnection(ConnectionString);
        }

        public override void Run<T>(IEnumerable<T> entities, SqlCeTransaction transaction, BulkInsertOptions options)
        {
            using (var adapter = new SqlCeDataAdapter())
            {
                var baseType = typeof(T);
            var allTypes = baseType.GetDerivedTypes(true);

            var neededMappings = allTypes.ToDictionary(x => x, x => Context.Db(x));

            var keepIdentity = (SqlBulkCopyOptions.KeepIdentity & options.SqlBulkCopyOptions) > 0;
                using (var reader = new MappedDataReader<T>(entities, neededMappings, keepIdentity))
                {

                    var commandText = string.Format("INSERT INTO {0} ({1}) VALUES ({2})", tableName, fields, parameters);

                    var cmd = new SqlCeCommand(commandText, (SqlCeConnection) transaction.Connection, transaction);

                    SqlCeParameter p = null;
                    p = cmd.Parameters.Add("@p0", SqlDbType.NVarChar, 10, "x");
                    p.SourceVersion = DataRowVersion.Original;

                    adapter.InsertCommand = cmd;
                    adapter.UpdateBatchSize = options.BatchSize;



                    var ds = new DataSet();
                    adapter.Fill(ds);

                    foreach (var entity in entities)
                    {
                        ds.Tables[0].Rows.Add(values);
                    }

                    // exec
                    adapter.Update(ds.Tables[0]);
                }
            }
        }
    }
}
