using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SchemaRegistry.Utils;

namespace SchemaRegistry.ConsoleTool
{
    class Program
    {
        static void Main(string[] args)
        {
            using (var registry = new SchemaRegistryApi("http://ecsc00104a5d.epam.com:8081/"))
            {
                var subjects = registry.GetAllSubjects();
                Console.WriteLine(subjects.ToJson());

                var versions = registry.GetSchemaVersions(subjects[0]);
                Console.WriteLine(versions.ToJson());

                var schema = registry.GetById(versions[0]);
                Console.WriteLine(schema.Schema);

                var schemaVersionSpecific = registry.GetBySubjectAndId(subjects[0], versions[0]);
                Console.WriteLine(schemaVersionSpecific.ToJson());

                var schemaVersionLatest = registry.GetLatestSchemaMetadata(subjects[0]);
                Console.WriteLine(schemaVersionLatest.ToJson());
            }
        }
    }
}
