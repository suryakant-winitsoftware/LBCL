// InterfaceConverter.cs
using System;
using System.Text.Json;
using System.Text.Json.Serialization;
namespace WINITMobile.Data
{
    public class InterfaceConverter<TInterface, TConcrete> : JsonConverter<TInterface>
        where TConcrete : TInterface
    {
        public override TInterface Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            return JsonSerializer.Deserialize<TConcrete>(ref reader, options);
        }

        public override void Write(Utf8JsonWriter writer, TInterface value, JsonSerializerOptions options)
        {
            JsonSerializer.Serialize(writer, (TConcrete)value, options);
        }
    }
}
