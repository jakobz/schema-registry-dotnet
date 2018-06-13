namespace SchemaRegistry.Messages
{
    public enum SchemaRegistryErrorCode
    {
        SubjectNotFound = 40401,
        VersionNotFound = 40402,
        SchemaNotFound = 40403,
        IncompatibleAvroSchema = 409,
        InvalidAvroSchema = 42201,
        InvalidVersion = 42202,
        InvalidCompatibilityLevel = 42203,
        ErrorInTheBackendDataStore = 50001,
        OperationTimedOut = 50002,
        ErrorWhileForwardingTheRequestToTheMaster = 50003
    }
}
