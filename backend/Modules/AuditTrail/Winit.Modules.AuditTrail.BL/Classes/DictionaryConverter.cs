using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Winit.Modules.Common.Model.Classes.AuditTrail;

namespace Winit.Modules.AuditTrail.BL.Classes
{
    public static class DictionaryConverter
    {
        public static Dictionary<string, object> ToDictionary(object obj)
        {
            if (obj == null) return new Dictionary<string, object>();

            return obj.GetType()
              .GetProperties(BindingFlags.Public | BindingFlags.Instance)
              .Where(prop => prop.CanRead && prop.GetCustomAttribute<AuditTrailAttribute>() != null) // Filter by AuditTrail
              .ToDictionary(
                  prop =>
                  {
                      var attr = prop.GetCustomAttribute<AuditTrailAttribute>();
                      return !string.IsNullOrEmpty(attr?.CustomName) ? attr.CustomName : prop.Name; // Use CustomName if provided
                  },
                  prop => ConvertValue(prop.GetValue(obj))
              );
        }

        private static object ConvertValue(object value)
        {
            if (value == null) return null;

            // Return primitive types as they are
            if (value is string || value is int || value is bool || value is decimal ||
                value is double || value is float || value is DateTime)
            {
                return value;
            }

            // If the value is a list, convert each item to a dictionary
            if (value is IEnumerable<object> list)
            {
                return list.Select(ToDictionary).ToList();
            }

            // If the value is a complex object, recursively convert it to a dictionary
            return ToDictionary(value);
        }
    }
}
