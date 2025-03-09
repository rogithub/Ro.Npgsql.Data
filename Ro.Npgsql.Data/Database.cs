using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Threading.Tasks;

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

        public Task ExecuteReader(DbCommand cmd, Action<IDataReader> action, CommandBehavior behavior = CommandBehavior.CloseConnection)
        {
            return DbTasks.ExecuteReaderAsync(cmd, GetConnection(), action, behavior);
        }

        public Task<T> GetOneRow<T>(DbCommand cmd, Func<IDataReader, T> mapper)
        {
            return DbTasks.GetOneRow(cmd, GetConnection(), mapper);
        }

        public Task<IEnumerable<T>> GetRows<T>(DbCommand cmd, Func<IDataReader, T> mapper)
        {
            return DbTasks.GetRows(cmd, GetConnection(), mapper);
        }

        public Task<T> GetOneRowAsync<T>(DbCommand cmd, Func<IDataReader, Task<T>> mapper)
        {
            return DbTasks.GetOneRowAsync(cmd, GetConnection(), mapper);
        }

        public Task<IEnumerable<T>> GetRowsAsync<T>(DbCommand cmd, Func<IDataReader, Task<T>> mapper)
        {
            return DbTasks.GetRowsAsync(cmd, GetConnection(), mapper);
        }
    }
}
