using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Data.Common;
using System;
using Npgsql;

namespace Ro.Npgsql.Data
{
    public static class CmdBuilderHelpers
    {
        public static DbType ToDbType(Type type)
        {
            return type switch
            {
                Type b when b == typeof(bool) => DbType.Boolean,
                Type byteT when byteT == typeof(byte) => DbType.Byte,
                Type sbyteT when sbyteT == typeof(sbyte) => DbType.SByte,
                Type shortT when shortT == typeof(short) => DbType.Int16,
                Type ushortT when ushortT == typeof(ushort) => DbType.UInt16,
                Type i when i == typeof(int) => DbType.Int32,
                Type uintT when uintT == typeof(uint) => DbType.UInt32,
                Type longT when longT == typeof(long) => DbType.Int64,
                Type ulongT when ulongT == typeof(ulong) => DbType.UInt64,
                Type f when f == typeof(float) => DbType.Single,
                Type d when d == typeof(double) => DbType.Double,
                Type dec when dec == typeof(decimal) => DbType.Decimal,
                Type date when date == typeof(DateTime) => DbType.DateTime,
                Type guid when guid == typeof(Guid) => DbType.Guid,
                Type str when str == typeof(string) => DbType.String,
                Type charT when charT == typeof(char) => DbType.String, // string for a single character
                Type byteArray when byteArray == typeof(byte[]) => DbType.Binary,
                Type dateOffset when dateOffset == typeof(DateTimeOffset) => DbType.DateTimeOffset,
                Type timeSpan when timeSpan == typeof(TimeSpan) => DbType.Time,
                _ => DbType.Object
            };
        }


        public static IDbDataParameter ToParam<T>(this T value, string name)
        {
            var type = value == null ? 
                Nullable.GetUnderlyingType(typeof(T)) 
                : value.GetType();
            
            var dbType = ToDbType(type);

            if (value == null)
            {
                return name.ToParam(dbType, DBNull.Value);
            }
            
            return name.ToParam(dbType, value);
        }
        
        public static DbCommand ToCmd(this string sql, params IDbDataParameter[] commandParameters)
        {
            DbCommand cmd = new NpgsqlCommand(sql);
            cmd.AddParams(commandParameters);
            return cmd;
        }
        public static DbCommand ToCmd(this string sql, CommandType type, params IDbDataParameter[] commandParameters)
        {
            var cmd = ToCmd(sql, commandParameters);
            cmd.CommandType = type;
            return cmd;
        }
        public static DbCommand ToCmd(this string sql, DbType type, object value, ParameterDirection direction = ParameterDirection.Input)
        {
            DbCommand cmd = ToCmd(sql);
            var name = sql.Split(' ').FirstOrDefault(param => param.StartsWith("@"));
            cmd.AddParams(name.Trim(), type, value, direction);
            return cmd;
        }

        public static IDbDataParameter ToParam(this string name, DbType type, object value, ParameterDirection direction = ParameterDirection.Input)
        {
            IDbDataParameter param = new NpgsqlParameter(name, value)
            {
                DbType = type,
                Direction = direction
            };

            return param;
        }

        public static void Add(this Dictionary<string, IDbDataParameter> d, string name, DbType type, object value)
        {
            d.Add(name, name.ToParam(type, value));
        }

        public static IDbCommand AddParams(this IDbCommand cmd, params IDbDataParameter[] commandParameters)
        {
            foreach (var p in commandParameters)
            {
                cmd.Parameters.Add(p);
            }
            return cmd;
        }

        public static IDbCommand AddParams(this IDbCommand cmd, string name, DbType type, object value, ParameterDirection direction = ParameterDirection.Input)
        {
            cmd.AddParams(name.ToParam(type, value, direction));
            return cmd;
        }
    }
}
