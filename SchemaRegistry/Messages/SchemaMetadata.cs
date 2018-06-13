namespace SchemaRegistry.Messages
{
    public class SchemaMetadata : SchemaContainer
    {
        public string Subject { get; set; }
        public int Version { get; set; }
    }
}
