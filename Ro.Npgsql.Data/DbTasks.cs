using System;
using System.Collections.Generic;
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

        public static async Task<object> ExecuteScalarAsync(DbCommand cmd, DbConnection conn)
        {
            try
            {
                cmd.Connection = conn;
                using (OpenConnection(cmd.Connection))
                {
                    using (cmd)
                    {
                        return await cmd.ExecuteScalarAsync();
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

        public static async Task<IEnumerable<T>> GetRows<T>(DbCommand cmd, DbConnection conn, Func<IDataReader, T> mapper)
        {
            List<T> list = new List<T>();
            await ExecuteReaderAsync(cmd, conn, (dr) => list.Add(mapper(dr)), CommandBehavior.SingleResult);
            return list;
        }

        public static async Task<IEnumerable<T>> GetRowsAsync<T>(DbCommand cmd, DbConnection conn, Func<IDataReader, Task<T>> mapper)
        {
            List<T> list = new List<T>();
            await ExecuteReaderAsync(cmd, conn, async (dr) => {
                list.Add(await mapper(dr));
            }, CommandBehavior.SingleResult);
            return list;
        }


        public static async Task ExecuteReaderAsync(DbCommand cmd, DbConnection conn, Action<IDataReader> action, CommandBehavior behavior = CommandBehavior.CloseConnection)
        {
            try
            {
                cmd.Connection = conn;
                using (OpenConnection(conn))
                {
                    using (cmd)
                    {
                        using (IDataReader dr = await cmd.ExecuteReaderAsync(behavior))
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
                        using (IDataReader dr = await cmd.ExecuteReaderAsync(CommandBehavior.SingleRow))
                        {
                            return dr.Read() ? mapper(dr) : default(T);
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
                        using (IDataReader dr = await cmd.ExecuteReaderAsync(CommandBehavior.SingleRow))
                        {
                            return dr.Read() ? await mapper(dr) : default(T);
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

        public static async Task<int> ExecuteNonQueryAsync(DbCommand cmd, DbConnection conn)
        {
            try
            {
                cmd.Connection = conn;
                using (OpenConnection(conn))
                {
                    using (cmd)
                    {
                        return await cmd.ExecuteNonQueryAsync();
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
