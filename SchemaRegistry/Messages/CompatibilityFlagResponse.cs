using Newtonsoft.Json;

namespace SchemaRegistry.Messages
{
    public class CompatibilityFlagResponse
    {
        [JsonProperty("is_compatible")]
        public bool IsCompatible { get; set; }
    }
}
