using System;
using System.Data;
using System.Xml;

namespace Ro.Npgsql.Data
{

    public static class Mappers
    {
        public static T FromDb<T>(this IDataReader dr, string name, T? defaultValue = default(T))
        {
            var isNullableType = Nullable.GetUnderlyingType(typeof (T)) != null;
            var value = dr[name];

            if (value == null || value == DBNull.Value)
            {
                return defaultValue!; 
            }

            return (T)value!;
        }
        
        public static Func<object, DateTime> ToDate = (o) =>
        {
            return Convert.ToDateTime(o);
        };

        public static Func<object, DateTime?> ToDateNullable = (o) =>
        {
            if (o == DBNull.Value)
                return null;
            return ToDate(o);
        };


        public static Func<object, string> ToStr = (o) =>
        {
            if (o == null || o == DBNull.Value)
                return string.Empty;
            
            return Convert.ToString(o) ?? string.Empty;
        };

        public static Func<object, decimal> ToDecimal = (o) =>
        {
            return (o == null) ? 0m : Convert.ToDecimal(o!);
        };

        public static Func<object, float> ToFloat = (o) =>
        {
            return (o == null) ? 0f : Convert.ToSingle(o!);
        };

        public static Func<object, int> ToInt = (o) =>
        {
            return (o == null) ? 0 : Convert.ToInt32(o!);
        };

        public static Func<object, long> ToLong = (o) =>
        {
            return (o == null) ? 0L : Convert.ToInt64(o!);
        };

        public static Func<object, Guid> ToGuid = (o) =>
        {
            return (Guid)(o);
        };

        public static Func<object, Guid?> ToGuidNullable = (o) =>
        {
            if (o == DBNull.Value)
                return null;
            return ToGuid(o);
        };

        public static Func<object, XmlDocument> ToXml = (o) =>
        {
            XmlDocument doc = new XmlDocument();
            doc.LoadXml(Mappers.ToStr(o));
            return doc;
        };

        public static T ToVal<T>(this object o)
        {
            return (T)o;
        }
    }
}
