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
