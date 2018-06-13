namespace SchemaRegistry.Messages
{
    public class ExistingSchemaResponse : SchemaContainer
    {
        public int Id { get; set; }
        public string Subject { get; set; }
        public int Version { get; set; }
    }
}
