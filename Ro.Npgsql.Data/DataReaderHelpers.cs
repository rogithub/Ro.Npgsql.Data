using System;
using System.Data;
using System.Xml;

namespace Ro.Npgsql.Data
{

    public static class DataReaderHelpers
    {
        public static DateTime GetDate(this IDataReader dr, string key)
        {
            return Mappers.ToDate(dr[key]);
        }

        public static string GetString(this IDataReader dr, string key)
        {
            return Mappers.ToStr(dr[key]);
        }

        public static decimal GetDecilmal(this IDataReader dr, string key)
        {
            return Mappers.ToDecimal(dr[key]);
        }

         public static float GetFloat(this IDataReader dr, string key)
        {
            return Mappers.ToFloat(dr[key]);
        }

        public static int GetInt(this IDataReader dr, string key)
        {
            return Mappers.ToInt(dr[key]);
        }

        public static long GetLong(this IDataReader dr, string key, long ifNull = 0)
        {
            return Mappers.ToLong(dr[key]);
        }

        public static Guid GetGuid(this IDataReader dr, string key)
        {
            return Mappers.ToGuid(dr[key]);
        }

        public static Guid? ToGuidNullable(this IDataReader dr, string key)
        {
            return Mappers.ToGuidNullable(dr[key]);
        }

        public static XmlDocument GetXml(this IDataReader dr, string key)
        {
            return Mappers.ToXml(dr[key]);
        }

        public static T GetValue<T>(this IDataReader dr, string key)
        {
            return Mappers.ToVal<T>(dr[key]);
        }
    }
}
