using System.Data.Common;

namespace Ro.Npgsql.Data
{
    public interface IDbConnectionFactory
    {
        DbConnection GetConnection();
    }
}
