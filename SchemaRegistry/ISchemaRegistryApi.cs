using System.Net.Http;
using System.Threading.Tasks;
using SchemaRegistry.Messages;

namespace SchemaRegistry
{
    /// <summary>
    /// Confluent Schema Registry API Endpoints, listed at http://docs.confluent.io/1.0.1/schema-registry/docs/api.html
    /// </summary>
    public interface ISchemaRegistryApi
    {
        /// <summary>
        /// Get the schema string identified by the input id.
        /// </summary>
        /// <param name="id">id (int) – the globally unique identifier of the schema</param>
        /// <returns>Response Object:
        /// - schema(string) – Schema string identified by the id
        /// </returns>
        Task<SchemaContainer> GetById(int id);

        /// <summary>
        /// Get a list of registered subjects.
        /// </summary>
        /// <returns>Array of subject names</returns>
        Task<string[]> GetAllSubjects();

        /// <summary>
        /// Get a list of versions registered under the specified subject.
        /// </summary>
        /// <param name="subject">the name of the subject</param>
        /// <returns>Array of versions of the schema registered under this subject</returns>
        Task<int[]> GetSchemaVersions(string subject);

        /// <summary>
        /// Get a specific version of the schema registered under this subject
        /// </summary>
        /// <param name="subject">Name of the subject</param>
        /// <param name="versionId">Version of the schema to be returned. Valid values for versionId are between [1,2^31-1]</param>
        Task<SchemaMetadata> GetBySubjectAndId(string subject, int versionId);

        /// <summary>
        /// Get a latest version of the schema registered under this subject (using /subjects/(string: subject)/versions/latest)
        /// Note that there may be a new latest schema that gets registered right after this request is served.
        /// </summary>
        /// <param name="subject">Name of the subject</param>
        Task<SchemaMetadata> GetLatestSchemaMetadata(string subject);

        /// <summary>
        /// Register a new schema under the specified subject. If successfully registered, this returns the unique identifier of this schema in the registry. The returned identifier should be used to retrieve this schema from the schemas resource and is different from the schema’s version which is associated with the subject. If the same schema is registered under a different subject, the same identifier will be returned. However, the version of the schema may be different under different subjects.
        /// A schema should be compatible with the previously registered schemas(if there are any) as per the configured compatibility level.The configured compatibility level can be obtained by issuing a GET http:get:: /config/(string: subject). If that returns null, then GET http:get:: /config
        /// When there are multiple instances of schema registry running in the same cluster, the schema registration request will be forwarded to one of the instances designated as the master.If the master is not available, the client will get an error code indicating that the forwarding has failed.
        /// </summary>
        /// <param name="subject">Subject under which the schema will be registered</param>
        /// <param name="schema">The Avro schema string</param>
        Task<int> Register(string subject, string schema);

        /// <summary>
        /// Check if a schema has already been registered under the specified subject. If so, this returns the schema string along with its globally unique identifier, its version under this subject and the subject name.
        /// </summary>
        /// <param name="subject">Subject under which the schema will be registered</param>
        Task<ExistingSchemaResponse> CheckIfSchemaRegistered(string subject, string schema);

        /// <summary>
        /// Test input schema against a particular version of a subject’s schema for compatibility. Note that the compatibility level applied for the check is the configured compatibility level for the subject (http:get:: /config/(string: subject)). If this subject’s compatibility level was never changed, then the global compatibility level applies (http:get:: /config).
        /// </summary>
        /// <param name="subject">Subject of the schema version against which compatibility is to be tested</param>
        /// <param name="versionId"> Version of the subject’s schema against which compatibility is to be tested. Valid values for versionId are between [1,2^31-1] </param>
        Task<bool> TestCompatibilityWithVersion(string subject, int versionId, string schema);

        /// <summary>
        /// Test input schema against a latest version of a subject’s schema for compatibility. Note that the compatibility level applied for the check is the configured compatibility level for the subject (http:get:: /config/(string: subject)). If this subject’s compatibility level was never changed, then the global compatibility level applies (http:get:: /config).
        /// </summary>
        /// <param name="subject">Subject of the schema version against which compatibility is to be tested</param>
        Task<bool> TestCompatibility(string subject, string schema);

        /// <summary>
        /// Update global compatibility level.
        /// When there are multiple instances of schema registry running in the same cluster, the update request will be forwarded to one of the instances designated as the master.If the master is not available, the client will get an error code indicating that the forwarding has failed.
        /// </summary>
        Task<CompatibilityLevel> GetGlobalConfig();

        /// <summary>
        /// Get global compatibility level.
        /// </summary>
        /// <param name="level">New global compatibility level.</param>
        Task<CompatibilityLevel> PutGlobalConfig(CompatibilityLevel level);

        /// <summary>
        /// Update compatibility level for the specified subject.
        /// </summary>
        /// <param name="subject">Name of the subject</param>
        /// <param name="level">New global compatibility level.</param>
        Task<CompatibilityLevel> PutSubjectConfig(string subject, CompatibilityLevel level);

        /// <summary>
        /// Get compatibility level for a subject.
        /// </summary>
        /// <param name="subject">Name of the subject</param>
        Task<CompatibilityLevel> GetSubjectConfig(string subject);
      
        /// <summary>
        /// Execute generic API request
        /// </summary>
        /// <typeparam name="TResponse">Response type to be parsed as JSON</typeparam>
        /// <param name="path">Relative API path</param>
        /// <param name="method">HTTP Method</param>
        /// <param name="payload">Object to pass as JSON to POST/PUT request</param>
        /// <returns>JSON-parsed response object</returns>
        Task<TResponse> RunRequest<TResponse, TRequest>(string path, HttpMethod method, TRequest payload);

        /// <summary>
        /// Execute generic API GET request
        /// </summary>
        /// <typeparam name="TResponse">Response type to be parsed as JSON</typeparam>
        /// <param name="path">Relative API path</param>
        /// <returns>JSON-parsed response object</returns>
        Task<TResponse> Get<TResponse>(string path);
    }
}