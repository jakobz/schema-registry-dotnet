using System;

namespace SchemaRegistry.Messages
{
    public class CompatibilityObject
    {
        public string Compatibility { get; set; }

        // For some reason, get requests return "compatibilityLevel", while PUT requests accepts and returns "compatibility" field
        public string CompatibilityLevel
        {
            set => Compatibility = value;
        }

        public static CompatibilityObject Create(CompatibilityLevel level)
        {
            if (level == Messages.CompatibilityLevel.NotSet)
            {
                return new CompatibilityObject
                {
                    Compatibility = null
                };
            }

            return new CompatibilityObject
            {
                Compatibility = Enum.GetName(typeof(CompatibilityLevel), level)?.ToUpperInvariant()
            };
        }
    }
}
