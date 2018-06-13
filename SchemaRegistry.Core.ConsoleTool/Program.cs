using System;
using System.Linq;

namespace SchemaRegistry.Core.ConsoleTool
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            using (var registry = new SchemaRegistryApi("http://schema-registry-sbox.epm-eco.projects.epam.com:8081"))
            {
                // Get first 10 subjects
                var subjects = registry.GetAllSubjects().Result.Take(10);
                Console.WriteLine("First 10 subjects: " + string.Join(", ", subjects));

                // Get last schema by subject
                var subject = subjects.First();
                var meta = registry.GetLatestSchemaMetadata(subject).Result;
                Console.WriteLine($"Last version of the {subject} subject: {meta.Version}");

                // Check schema compatibiliy
                var isCompatible = registry.TestCompatibility(subject, meta.Schema).Result;
                Console.WriteLine($"Is the schema compatible to itself: {isCompatible}");

                // Register the schema (returns the same ID for identical schema)
                var newSchemaId = registry.Register(subject, meta.Schema).Result;
                Console.WriteLine($"Schema id: {newSchemaId}");
            }
        }
    }
}