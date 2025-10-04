using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using System.Text.Json;
namespace Winit.Modules.AuditTrail.BL.Classes
{
    public class AuditTrailCommonFunctions
    {
        public static T ConvertToBsonDeserializedData<T>(object inputObject)
        {
            if (inputObject == null)
            {
                throw new ArgumentNullException(nameof(inputObject), "Input object cannot be null.");
            }

            try
            {
                // Serialize the input object to JSON
                //string serializedJson = System.Text.Json.JsonSerializer.Serialize(inputObject);

                //// Deserialize the JSON into the specified type T using BsonSerializer
                //T bsonDeserializedData = BsonSerializer.Deserialize<T>(serializedJson);


                string serializedJson = Newtonsoft.Json.JsonConvert.SerializeObject(inputObject);

                // Deserialize the JSON into the specified type T using BsonSerializer
                T bsonDeserializedData = BsonSerializer.Deserialize<T>(serializedJson);
                return bsonDeserializedData;
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Failed to convert object to BSON deserialized data of type {typeof(T)}.", ex);
            }
        }

        public static BsonValue ConvertJsonElementToBsonDocument(JsonElement element)
        {
            switch (element.ValueKind)
            {
                case JsonValueKind.Object:
                    var bsonDoc = new BsonDocument();
                    foreach (var property in element.EnumerateObject())
                    {
                        bsonDoc.Add(property.Name, ConvertJsonElementToBsonDocument(property.Value));
                    }
                    return new BsonDocumentWrapper(bsonDoc); // Wrap with BsonDocumentWrapper

                case JsonValueKind.Array:
                    var bsonArray = new BsonArray();
                    foreach (var item in element.EnumerateArray())
                    {
                        bsonArray.Add(ConvertJsonElementToBsonDocument(item));
                    }
                    return bsonArray;

                case JsonValueKind.String:
                    return element.GetString();

                case JsonValueKind.Number:
                    return element.TryGetInt64(out var longValue) ? longValue : element.GetDouble();

                case JsonValueKind.True:
                case JsonValueKind.False:
                    return element.GetBoolean();

                case JsonValueKind.Null:
                    return BsonNull.Value;

                default:
                    return element.GetRawText(); // Handle any unexpected cases
            }
        }

    }
}
