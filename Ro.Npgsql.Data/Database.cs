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
        private readonly IDbConnectionFactory _dbConnectionFactory;

        public Database(string connString)
        {
            _dbConnectionFactory = new NpgsqlConnectionFactory(connString);
        }

        public Database(IDbConnectionFactory factory)
        {
            _dbConnectionFactory = factory;
        }

        private class NpgsqlConnectionFactory : IDbConnectionFactory
        {
            private readonly string _connectionString;
            public NpgsqlConnectionFactory(string connectionString)
            {
                _connectionString = connectionString;
            }
            public DbConnection GetConnection()
            {
                return new NpgsqlConnection(_connectionString);
            }
        }

        public Task<int> ExecuteNonQuery(DbCommand cmd)
        {
            return DbTasks.ExecuteNonQueryAsync(cmd, _dbConnectionFactory.GetConnection());
        }

        public Task<object?> ExecuteScalar(DbCommand cmd)
        {
            return DbTasks.ExecuteScalarAsync(cmd, _dbConnectionFactory.GetConnection());
        }

        public Task ExecuteReader(DbCommand cmd, Action<IDataReader> action, CommandBehavior behavior = CommandBehavior.CloseConnection)
        {
            return DbTasks.ExecuteReaderAsync(cmd, _dbConnectionFactory.GetConnection(), action, behavior);
        }

        public Task<T?> GetOneRow<T>(DbCommand cmd, Func<IDataReader, T> mapper) where T : class
        {
            return DbTasks.GetOneRow(cmd, _dbConnectionFactory.GetConnection(), mapper);
        }

        public Task<IEnumerable<T>> GetRows<T>(DbCommand cmd, Func<IDataReader, T> mapper)
        {
            return DbTasks.GetRows(cmd, _dbConnectionFactory.GetConnection(), mapper);
        }

        public Task<T?> GetOneRowAsync<T>(DbCommand cmd, Func<IDataReader, Task<T>> mapper) where T : class
        {
            return DbTasks.GetOneRowAsync(cmd, _dbConnectionFactory.GetConnection(), mapper);
        }

        public Task<IEnumerable<T>> GetRowsAsync<T>(DbCommand cmd, Func<IDataReader, Task<T>> mapper)
        {
            return DbTasks.GetRowsAsync(cmd, _dbConnectionFactory.GetConnection(), mapper);
        }
    }
}
