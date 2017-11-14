using System;

namespace SchemaRegistry.Core.ConsoleTool
{
    class Program
    {
        static void Main(string[] args)
        {
            var registry = new SchemaRegistryApi("http://schema-registry-sbox.epm-eco.projects.epam.com:8081");
            {
                var subjects = registry.GetAllSubjects().Result;
                Console.WriteLine(String.Join(", ", subjects));

                var subject = subjects[0];
                var meta = registry.GetLatestSchemaMetadata(subject).Result;
                Console.WriteLine(meta.Version);
                var isCompatible = registry.TestCompatibility(subject, meta.Schema).Result;
                Console.WriteLine(isCompatible);
            }
        }
    }
}