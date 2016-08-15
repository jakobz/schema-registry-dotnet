using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SchemaRegistry.Utils
{
    public static class JsonUtils
    {
        private static readonly JsonSerializerSettings JsonSerializerSettings = new JsonSerializerSettings
        {
            ContractResolver = new CamelCasePropertyNamesContractResolver(),
            DateFormatHandling = DateFormatHandling.IsoDateFormat,
            Formatting = Formatting.Indented
        };

        public static JsonSerializerSettings SerializerSettings => JsonSerializerSettings;

        public static readonly JsonSerializer Serializer = JsonSerializer.Create(JsonSerializerSettings);

        public static string ToJson(this object obj)
        {
            return obj == null ? "" : JsonConvert.SerializeObject(obj, JsonSerializerSettings);
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
