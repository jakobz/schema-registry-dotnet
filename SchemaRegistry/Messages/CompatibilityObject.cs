using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SchemaRegistry.Messages
{
    public class CompatibilityObject
    {
        public string Compatibility { get; set; }
        public static CompatibilityObject Create(CompatibilityLevel level)
        {
            return new CompatibilityObject { Compatibility = Enum.GetName(typeof(CompatibilityLevel), level).ToUpperInvariant() };
        }
    }
}
