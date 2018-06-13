using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System.IO;

namespace SchemaRegistry.Utils
{
    public static class JsonUtils
    {
        public static JsonSerializerSettings SerializerSettings { get; } = new JsonSerializerSettings
        {
            ContractResolver = new CamelCasePropertyNamesContractResolver(),
            DateFormatHandling = DateFormatHandling.IsoDateFormat,
            Formatting = Formatting.Indented
        };

        public static readonly JsonSerializer Serializer = JsonSerializer.Create(SerializerSettings);

        public static string ToJson(this object obj)
        {
            return obj == null ? "" : JsonConvert.SerializeObject(obj, SerializerSettings);
        }

        public static T FromJson<T>(string json)
        {
            return JsonConvert.DeserializeObject<T>(json);
        }

        public static T FromJsonStream<T>(Stream stream)
        {
            using (var streamReader = new StreamReader(stream))
            {
                var responseText = streamReader.ReadToEnd();
                return FromJson<T>(responseText);
            }
        }

        public static void ToJsonStream<T>(T obj, Stream stream)
        {
            using (var streamWriter = new StreamWriter(stream))
            {
                var json = ToJson(obj);
                streamWriter.Write(json);
                streamWriter.Flush();
            }
        }
    }
}
