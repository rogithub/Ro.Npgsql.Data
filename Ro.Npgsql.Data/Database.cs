using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Threading.Tasks;
using Npgsql;

namespace Ro.Npgsql.Data
{
    public class Database : IDbAsync
    {
        private string ConnectionString { get; set; }
        public Database(string connString)
        {
            this.ConnectionString = connString;
        }

        private DbConnection GetConnection()
        {
            return new NpgsqlConnection(this.ConnectionString);
        }

        public Task<int> ExecuteNonQuery(DbCommand cmd)
        {
            return DbTasks.ExecuteNonQueryAsync(cmd, GetConnection());
        }

        public Task<object> ExecuteScalar(DbCommand cmd)
        {
            return DbTasks.ExecuteScalarAsync(cmd, GetConnection());
        }

        public Task ExecuteReader(DbCommand cmd, Action<IDataReader> action)
        {
            return DbTasks.ExecuteReaderAsync(cmd, GetConnection(), action);
        }

        public Task<T> GetOneRow<T>(DbCommand cmd, Func<IDataReader, T> mapper)
        {
            return DbTasks.GetOneRow(cmd, GetConnection(), mapper);
        }

        public async Task<IEnumerable<T>> GetRows<T>(DbCommand cmd, Func<IDataReader, T> mapper)
        {
            List<T> list = new List<T>();
            await DbTasks.ExecuteReaderAsync(cmd, GetConnection(), (dr) => list.Add(mapper(dr)));
            return list;
        }
    }
}
