using System;

namespace SchemaRegistry.Core.ConsoleTool
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            var registry = new SchemaRegistryApi("http://schema-registry-sbox.epm-eco.projects.epam.com:8081");
            {
                var subjects = registry.GetAllSubjects().Result;
                Console.WriteLine(string.Join(", ", subjects));
            }
        }
    }
}