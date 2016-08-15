using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SchemaRegistry.Messages
{
    public class CompatibilityFlagResponse
    {
        [JsonProperty("is_compatible")]
        public bool IsCompatible { get; set; }
    }
}
