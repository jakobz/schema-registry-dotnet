using Newtonsoft.Json;

namespace SchemaRegistry.Messages
{
    public class SchemaRegistryError
    {
        [JsonProperty("error_code")]
        public int ErrorCode { get; set; }
        public string Message { get; set; }
    }
}
