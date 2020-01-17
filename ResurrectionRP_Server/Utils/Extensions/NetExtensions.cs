using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Runtime.CompilerServices;

namespace ResurrectionRP_Server
{
    public static class NetExtensions
    {
        [MethodImpl(MethodImplOptions.Synchronized)]
        public static TValue GetOrAdd<TKey, TValue>(this Dictionary<TKey, TValue> hashtable, TKey key, TValue value) where TValue : class
        {
            if (hashtable.TryGetValue(key, out TValue valOut))
                return valOut;
            hashtable.Add(key, value);
            return value;
        }

        public static bool HasInterface(this Type t, Type interfaceType)
        {
            return t.GetInterfaces().Contains(interfaceType);
        }

        public static T GetArg<T>(this IEnumerable<object> args, int index, T defaultValue = default)
        {
            var tmpList = args?.ToList();
            if ((args == null) || (index >= tmpList.Count))
                return defaultValue;
            try
            {
                if (typeof(T).IsEnum)
                    return (T)Enum.Parse(typeof(T), tmpList[index].ToString());
                if (typeof(T) is IConvertible)
                    return (T)Convert.ChangeType(tmpList[index], typeof(T), CultureInfo.InvariantCulture);
                return (T)tmpList[index];
            }
            catch { return defaultValue; }
        }

        public static T GetArg<T>(this object[] args, int index, T defaultValue = default)
        {
            var tmpList = args?.ToList();
            if ((args == null) || (index >= tmpList.Count))
                return defaultValue;
            try
            {
                if (typeof(T).IsEnum)
                    return (T)Enum.Parse(typeof(T), tmpList[index].ToString());
                if (typeof(T) is IConvertible)
                    return (T)Convert.ChangeType(tmpList[index], typeof(T), CultureInfo.InvariantCulture);
                return (T)tmpList[index];
            }
            catch { return defaultValue; }
        }
    }
}
