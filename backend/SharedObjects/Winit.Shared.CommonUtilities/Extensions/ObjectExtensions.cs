using Nest;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.PortableExecutable;
using System.Text;
using System.Threading.Tasks;
using Winit.Shared.CommonUtilities.Common;

namespace Winit.Shared.CommonUtilities.Extensions
{
    public static class ObjectExtensions
    {

        public static T? GetPropertyValue<T>(this object item, string propertyName)
        {
            try
            {
                if (item != null)
                {
                    var property = item.GetType().GetProperty(propertyName);
                    if (property != null)
                    {
                        object? value = property.GetValue(item);
                        //return (T)property.GetValue(item);
                        if (value != null)
                        {
                            return (T)value;
                        }
                        else
                        {
                            value = default(T);// Assign default value for non-nullable types
                        }
                        /*
                        object? value;
                        var targetType = Nullable.GetUnderlyingType(property.PropertyType) ?? property.PropertyType;

                        if (targetType == typeof(int) || targetType == typeof(Int32))
                        {
                            value = CommonFunctions.GetIntValue(property.GetValue(item)??0);
                        }
                        else if (targetType == typeof(decimal) || targetType == typeof(decimal?))
                        {
                            value = CommonFunctions.GetDecimalValue(property.GetValue(item) ?? 0);
                        }
                        else if (targetType == typeof(bool) || targetType == typeof(bool?))
                        {
                            value = CommonFunctions.GetBooleanValue(property.GetValue(item) ?? 0);
                        }
                        else if (targetType == typeof(DateTime) || targetType == typeof(DateTime?))
                        {
                            value = (DateTime)(property.GetValue(item)?? DateTime.MinValue);
                        }
                        else
                        {
                            value = property.GetValue(item) ?? default;
                        }

                        if (value != null)
                        {
                            return (T)value;
                        }
                        else
                        {
                            value = default(T); // Assign default value for non-nullable types
                        }
                        */
                    }
                }
                return default(T);// Property not found
            }
            catch (Exception ex)
            {
                throw;
            }
        }
        public static T? DeepCopy<T>(this T self)
        {
            var serialized = JsonConvert.SerializeObject(self);
            return JsonConvert.DeserializeObject<T>(serialized);
        }

        public static T? DeepCopy<T>(this T self, Type? type = null, Newtonsoft.Json.JsonSerializerSettings jsonSerializerSettings = null)
        {
            string? serialized = JsonConvert.SerializeObject(self);
            if (jsonSerializerSettings != null)
            {
                return JsonConvert.DeserializeObject<T>(serialized, jsonSerializerSettings);
            }
            else if (type != null)
            {
                return (T?)JsonConvert.DeserializeObject(serialized, type);
                ;
            }
            return JsonConvert.DeserializeObject<T>(serialized);
        }
    }
}
