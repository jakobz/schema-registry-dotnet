using NUnit.Framework;
using SchemaRegistry.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SchemaRegistry.Tests
{
    [TestFixture]
    public class SchemaRegistryApiTests
    {
        public void CanReadSchemas()
        {
            using (var registry = TestsConfig.GetRegistryApi())
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
