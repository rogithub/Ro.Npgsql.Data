using System;
using System.Linq;
using System.Data;
using System.Data.Common;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace Ro.Npgsql.Data
{
    public interface IDbAsync
    {
        Task<int> ExecuteNonQuery(DbCommand cmd);
        Task<object> ExecuteScalar(DbCommand cmd);
        Task ExecuteReader(DbCommand cmd, Action<IDataReader> mapper);
        Task<T> GetOneRow<T>(DbCommand cmd, Func<IDataReader, T> mapper);
        Task<IEnumerable<T>> GetRows<T>(DbCommand cmd, Func<IDataReader, T> mapper);
    }
}
