using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Text.Json;
using System.Threading.Tasks;
using System.Reflection;

namespace Winit.Modules.Common.Model.Classes.AuditTrail
{
    public class AuditTrailConverter<T> : JsonConverter<T>
    {
        public override T Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            throw new NotImplementedException("Deserialization is not supported.");
        }

        public override void Write(Utf8JsonWriter writer, T value, JsonSerializerOptions options)
        {
            if (value == null)
            {
                writer.WriteNullValue();
                return;
            }

            writer.WriteStartObject();

            // Use the runtime type of the object, not just T
            var runtimeType = value.GetType();
            var properties = runtimeType.GetProperties();

            foreach (var property in properties)
            {
                // Check if the property has the AuditTrail attribute
                if (Attribute.IsDefined(property, typeof(AuditTrailAttribute)))
                {
                    var propValue = property.GetValue(value);

                    var auditTrailAttr = property.GetCustomAttribute<AuditTrailAttribute>();
                    var propertyName = !string.IsNullOrEmpty(auditTrailAttr?.CustomName) ? auditTrailAttr.CustomName : property.Name;

                    writer.WritePropertyName(propertyName);

                    if (propValue == null)
                    {
                        writer.WriteNullValue();
                    }
                    else if (typeof(System.Collections.IEnumerable).IsAssignableFrom(property.PropertyType) && property.PropertyType != typeof(string))
                    {
                        // Handle collections (like List<T>)
                        writer.WriteStartArray();
                        foreach (var item in (System.Collections.IEnumerable)propValue)
                        {
                            if (item != null)
                            {
                                JsonSerializer.Serialize(writer, item, new JsonSerializerOptions
                                {
                                    Converters = { Activator.CreateInstance(typeof(AuditTrailConverter<>).MakeGenericType(item.GetType())) as JsonConverter }
                                });
                            }
                            else
                            {
                                writer.WriteNullValue();
                            }
                        }
                        writer.WriteEndArray();
                    }
                    else if ((property.PropertyType.IsClass && property.PropertyType != typeof(string)) 
                        || property.PropertyType.IsInterface)
                    {
                        var actualType = propValue.GetType(); // Get the concrete type at runtime
                        JsonSerializer.Serialize(writer, propValue, new JsonSerializerOptions
                        {
                            Converters = { Activator.CreateInstance(typeof(AuditTrailConverter<>).MakeGenericType(actualType)) as JsonConverter }
                        });
                    }
                    else
                    {
                        // Handle primitive types
                        JsonSerializer.Serialize(writer, propValue, options);
                    }
                }
            }

            writer.WriteEndObject();
        }
    }
}
