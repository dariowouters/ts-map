using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using TsMap.TsItem;

namespace TsMap.Utils
{
    internal static class MapSiiUnitToClass
    {
        public static T Parse<T>(byte[] data)
        {
            T obj = Activator.CreateInstance<T>();

            var lines = Encoding.UTF8.GetString(data).Split('\n');

            foreach (var line in lines)
            {
                if (line == "") continue;
                if (line.TrimStart().StartsWith("#")) continue;
                if (line.TrimStart().StartsWith("{")) continue;
                if (line.TrimStart().StartsWith("}")) continue;

                KeyValuePair<string, string> content = ReadSiiKeyPair(line);
                if (content.Key != null)
                {
                    SetFieldValue<T>(obj, content.Key, content.Value.Replace("@", string.Empty).Replace("}", string.Empty));
                }
            }

            return obj;
        }

        private static void SetFieldValue<T>(T instance, string fieldName, string value)
        {
            var props = typeof(T).GetProperties().ToList();

            foreach (PropertyInfo prop in props)
            {
                if (prop.GetCustomAttribute<SiiUnitFieldAttribute>() != null && prop.GetCustomAttribute<SiiUnitFieldAttribute>().Name.Equals(fieldName, StringComparison.InvariantCultureIgnoreCase))
                {
                    if (prop.PropertyType.IsAssignableFrom(typeof(List<string>)))
                    {
                        var list = prop.GetValue(instance);
                        prop.PropertyType.GetMethod("Add").Invoke(list, new[] { value });
                    }
                    else
                    {
                        object convertedValue = value;

                        switch (prop.PropertyType.FullName)
                        {
                            case "System.Decimal":
                                convertedValue = Convert.ToDecimal(value, new CultureInfo("en_US"));
                                break;
                            case "System.Boolean":
                                convertedValue = value == "true";
                                break;
                            case "System.Int32":
                                if (value.Contains("."))
                                {
                                    convertedValue = Convert.ToInt32(Convert.ToDecimal(value));
                                }
                                else
                                {
                                    convertedValue = Convert.ToInt32(value);
                                }
                                break;
                        }

                        prop.SetValue(instance, convertedValue);
                    }
                    return;
                }
            }
        }

        public static bool IsGenericList(PropertyInfo p)
        {
            return p.PropertyType.IsGenericType && typeof(Collection<>).IsAssignableFrom(p.PropertyType.GetGenericTypeDefinition());
        }

        private static KeyValuePair<string, string> ReadSiiKeyPair(string line)
        {
            string[] segments = line.Split(':');
            if (segments.Length == 2)
            {
                KeyValuePair<string, string> ret = new KeyValuePair<string, string>(segments[0].Trim(), segments[1].Trim().Replace("\"", string.Empty));
                return ret;
            }
            else
                return new KeyValuePair<string, string>();
        }

        public static object ConvertType(Type type, string input)
        {
            object result = default(object);
            var converter = TypeDescriptor.GetConverter(type);
            if (converter != null)
            {
                try
                {
                    result = converter.ConvertFromString(input);
                }
                catch
                {
                    // add you exception handling
                }
            }
            return result;
        }
    }
}
