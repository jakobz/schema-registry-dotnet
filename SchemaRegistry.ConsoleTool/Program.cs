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
                var schema = registry.GetLatestSchemaMetadata("test_avro_X1").Schema;
                Console.WriteLine(schema.ToJson());
            }
        }
    }
}
