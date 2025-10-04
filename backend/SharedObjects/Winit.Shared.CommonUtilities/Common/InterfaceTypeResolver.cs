using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace Winit.Shared.CommonUtilities.Common;


public class InterfaceTypeResolver : JsonConverter
{
    private readonly IServiceProvider _serviceProvider;

    public InterfaceTypeResolver(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
    }

    public override bool CanConvert(Type objectType)
    {
        return !string.IsNullOrEmpty(objectType.Namespace) && !objectType.Namespace.StartsWith("System") && (objectType.IsInterface && _serviceProvider.GetService(objectType) != null);
    }

    public override object? ReadJson(JsonReader reader, Type objectType, object? existingValue, JsonSerializer serializer)
    {
        if (!string.IsNullOrEmpty(objectType.Namespace) && !objectType.Namespace.StartsWith("System") && (objectType.IsInterface && _serviceProvider.GetService(objectType) != null))
        {
            var service = _serviceProvider.GetService(objectType);
            if (service != null)
            {
                return serializer.Deserialize(reader, service.GetType());
            }
        }
        return serializer.Deserialize(reader, objectType);
    }

    public override void WriteJson(JsonWriter writer, object? value, JsonSerializer serializer)
    {
        if (value != null)
        {
            serializer.Serialize(writer, value, value.GetType());
        }
        else
        {
            writer.WriteNull();
        }
    }
}
