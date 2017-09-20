using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SchemaRegistry.Tests
{
    public class TestsConfig
    {
        public static ISchemaRegistryApi GetRegistryApi()
        {
            return new SchemaRegistryApi("http://schema-registry-sbox.epm-eco.projects.epam.com:8081/");
        }
    }
}
