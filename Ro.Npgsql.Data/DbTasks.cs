using System;
using System.Data;
using System.Data.Common;
using System.Threading.Tasks;

namespace Ro.Npgsql.Data
{
    internal abstract class DbTasks
    {
        public DbTasks()
        {

        }

        protected static IDbConnection OpenConnection(IDbConnection conn)
        {
            if (conn.State != System.Data.ConnectionState.Open)
            {
                conn.Open();
            }
            return conn;
        }

        protected static IDbConnection CloseConnection(IDbConnection conn)
        {
            if (conn.State != System.Data.ConnectionState.Closed)
            {
                conn.Close();
            }
            return conn;
        }

        public static Task<object> ExecuteScalarAsync(DbCommand cmd, DbConnection conn)
        {
            try
            {
                cmd.Connection = conn;
                using (OpenConnection(cmd.Connection))
                {
                    using (cmd)
                    {
                        return cmd.ExecuteScalarAsync();
                    }
                }
            }
            catch
            {
                throw;
            }
            finally
            {
                CloseConnection(conn);
            }
        }

        public static async Task ExecuteReaderAsync(DbCommand cmd, DbConnection conn, Action<IDataReader> action)
        {
            try
            {
                cmd.Connection = conn;
                using (OpenConnection(conn))
                {
                    using (cmd)
                    {
                        using (IDataReader dr = await cmd.ExecuteReaderAsync(CommandBehavior.CloseConnection))
                        {
                            while (dr.Read())
                            {
                                action(dr);
                            }
                        }
                    }
                }
            }
            catch
            {
                throw;
            }
            finally
            {
                CloseConnection(conn);
            }
        }

        public static async Task<T> GetOneRow<T>(DbCommand cmd, DbConnection conn, Func<IDataReader, T> mapper)
        {
            try
            {
                cmd.Connection = conn;
                using (OpenConnection(conn))
                {
                    using (cmd)
                    {
                        using (IDataReader dr = await cmd.ExecuteReaderAsync(CommandBehavior.CloseConnection))
                        {
                            if (dr.Read())
                            {
                                return mapper(dr);
                            }

                            return default(T);
                        }
                    }
                }
            }
            catch
            {
                throw;
            }
            finally
            {
                CloseConnection(conn);
            }
        }

        public static async Task<T> GetOneRowAsync<T>(DbCommand cmd, DbConnection conn, Func<IDataReader, Task<T>> mapper)
        {
            try
            {
                cmd.Connection = conn;
                using (OpenConnection(conn))
                {
                    using (cmd)
                    {
                        using (IDataReader dr = await cmd.ExecuteReaderAsync(CommandBehavior.CloseConnection))
                        {
                            if (dr.Read())
                            {
                                return await mapper(dr);
                            }

                            return default(T);
                        }
                    }
                }
            }
            catch
            {
                throw;
            }
            finally
            {
                CloseConnection(conn);
            }
        }

        public static Task<int> ExecuteNonQueryAsync(DbCommand cmd, DbConnection conn)
        {
            try
            {
                cmd.Connection = conn;
                using (OpenConnection(conn))
                {
                    using (cmd)
                    {
                        return cmd.ExecuteNonQueryAsync();
                    }
                }
            }
            catch
            {
                throw;
            }
            finally
            {
                CloseConnection(conn);
            }
        }
    }
}
