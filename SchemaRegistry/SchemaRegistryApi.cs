using SchemaRegistry.Messages;
using SchemaRegistry.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace SchemaRegistry
{
    public class SchemaRegistryApi : IDisposable, ISchemaRegistryApi
    {
        private string _registryUrl;
        private WebClient _client;

        public SchemaRegistryApi(string url)
        {
            _registryUrl = url.TrimEnd('/');
        }

        // Generic API wrappers

        /// <summary>
        /// Execute API request
        /// </summary>
        /// <typeparam name="TResponse">Response type to be parsed as JSON</typeparam>
        /// <param name="path">Relative API path</param>
        /// <param name="method">HTTP Method</param>
        /// <param name="payload">Object to pass as JSON to POST/PUT request</param>
        /// <returns>JSON-parsed response object</returns>
        public TResponse RunRequest<TResponse, TRequest>(string path, HttpMethod method, TRequest payload)
        {
            if (_client == null)
            {
                _client = new WebClient();
                _client.Headers.Add("Accept", "application/vnd.schemaregistry.v1+json");
            }

            string url = _registryUrl + path;
            string responseString = null;

            try
            {
                if (method == HttpMethod.Get)
                {
                    responseString = _client.DownloadString(url);
                }
                else
                {
                    responseString = _client.UploadString(url, method.Method, payload != null ? JsonUtils.ToJson(payload) : "");
                }

                return JsonUtils.FromJson<TResponse>(responseString);
            }
            catch(WebException webException)
            {
                SchemaRegistryError error = null;
                try
                {
                    error = JsonUtils.FromJson<SchemaRegistryError>(responseString);
                }
                catch
                {
                }

                throw new SchemaRegistryException(error, webException);
            }
        }

        /// <summary>
        /// Execute API GET request
        /// </summary>
        /// <typeparam name="TResponse">Response type to be parsed as JSON</typeparam>
        /// <param name="path">Relative API path</param>
        /// <returns>JSON-parsed response object</returns>
        public TResponse Get<TResponse>(string path)
        {
            return RunRequest<TResponse, string>(path, HttpMethod.Get, null);
        }


        /// <summary>
        /// Execute API POST request
        /// </summary>
        /// <typeparam name="TResponse">Response type to be parsed as JSON</typeparam>
        /// <param name="path">Relative API path</param>
        /// <returns>JSON-parsed response object</returns>
        public TResponse Post<TResponse>(string path)
        {
            var response = _client.DownloadString(_registryUrl + path);
            return JsonUtils.FromJson<TResponse>(response);
        }



        // API Endpoints, as listed at http://docs.confluent.io/1.0.1/schema-registry/docs/api.html


        /// <summary>
        /// Get the schema string identified by the input id.
        /// </summary>
        /// <param name="id">id (int) – the globally unique identifier of the schema</param>
        /// <returns>Response Object:
        /// - schema(string) – Schema string identified by the id
        /// </returns>
        /// <exception cref="Exception">Test 123</exception>        
        public SchemaContainer GetById(int id)
        {
            return Get<SchemaContainer>($"/schemas/ids/{id}");
        }

        /// <summary>
        /// Get a list of registered subjects.
        /// </summary>
        /// <returns>Array of subject names</returns>
        public string[] GetAllSubjects()
        {
            return Get<string[]>("/subjects");
        }

        /// <summary>
        /// Get a list of versions registered under the specified subject.
        /// </summary>
        /// <param name="subject">the name of the subject</param>
        /// <returns>Array of versions of the schema registered under this subject</returns>
        public int[] GetSchemaVersions(string subject)
        {
            return Get<int[]>($"/subjects/{subject}/versions");
        }

        /// <summary>
        /// Get a specific version of the schema registered under this subject
        /// </summary>
        /// <param name="subject">Name of the subject</param>
        /// <param name="versionId">Version of the schema to be returned. Valid values for versionId are between [1,2^31-1]</param>
        public SchemaMetadata GetBySubjectAndId(string subject, int versionId)
        {
            return Get<SchemaMetadata>($"/subjects/{subject}/versions/{versionId}");
        }

        /// <summary>
        /// Get a latest version of the schema registered under this subject (using /subjects/(string: subject)/versions/latest)
        /// Note that there may be a new latest schema that gets registered right after this request is served.
        /// </summary>
        /// <param name="subject">Name of the subject</param>
        public SchemaMetadata GetLatestSchemaMetadata(string subject)
        {
            return Get<SchemaMetadata>($"/subjects/{subject}/versions/latest");
        }

        /// <summary>
        /// Register a new schema under the specified subject. If successfully registered, this returns the unique identifier of this schema in the registry. The returned identifier should be used to retrieve this schema from the schemas resource and is different from the schema’s version which is associated with the subject. If the same schema is registered under a different subject, the same identifier will be returned. However, the version of the schema may be different under different subjects.
        /// A schema should be compatible with the previously registered schemas(if there are any) as per the configured compatibility level.The configured compatibility level can be obtained by issuing a GET http:get:: /config/(string: subject). If that returns null, then GET http:get:: /config
        /// When there are multiple instances of schema registry running in the same cluster, the schema registration request will be forwarded to one of the instances designated as the master.If the master is not available, the client will get an error code indicating that the forwarding has failed.
        /// </summary>
        /// <param name="subject">Subject under which the schema will be registered</param>
        /// <param name="schema">The Avro schema string</param>
        public int Register(string subject, string schema)
        {
            return RunRequest<IdContainer, SchemaContainer>($"/subjects/{subject}/versions", HttpMethod.Post, new SchemaContainer { Schema = schema }).Id;
        }

        /// <summary>
        /// Check if a schema has already been registered under the specified subject. If so, this returns the schema string along with its globally unique identifier, its version under this subject and the subject name.
        /// </summary>
        /// <param name="subject">Subject under which the schema will be registered</param>
        public ExistingSchemaResponse CheckIfSchemaRegistered(string subject, string schema)
        {
            return RunRequest<ExistingSchemaResponse, SchemaContainer>($"/subjects/{subject}", HttpMethod.Post, new SchemaContainer { Schema = schema });
        }

        /// <summary>
        /// Test input schema against a particular version of a subject’s schema for compatibility. Note that the compatibility level applied for the check is the configured compatibility level for the subject (http:get:: /config/(string: subject)). If this subject’s compatibility level was never changed, then the global compatibility level applies (http:get:: /config).
        /// </summary>
        /// <param name="subject">Subject of the schema version against which compatibility is to be tested</param>
        /// <param name="versionId"> Version of the subject’s schema against which compatibility is to be tested. Valid values for versionId are between [1,2^31-1] </param>
        public bool TestCompatibilityWithVersion(string subject, int versionId, string schema)
        {
            return RunRequest<bool, SchemaContainer>($"/compatibility/subjects/{subject}/versions/{versionId}", HttpMethod.Post, new SchemaContainer { Schema = schema });
        }

        /// <summary>
        /// Test input schema against a latest version of a subject’s schema for compatibility. Note that the compatibility level applied for the check is the configured compatibility level for the subject (http:get:: /config/(string: subject)). If this subject’s compatibility level was never changed, then the global compatibility level applies (http:get:: /config).
        /// </summary>
        /// <param name="subject">Subject of the schema version against which compatibility is to be tested</param>
        public bool TestCompatibility(string subject, string schema)
        {
            return RunRequest<CompatibilityFlagResponse, SchemaContainer>(
                $"/compatibility/subjects/{subject}/versions/latest",
                HttpMethod.Post,
                new SchemaContainer { Schema = schema }).IsCompatible;
        }

        /// <summary>
        /// Update global compatibility level.
        /// When there are multiple instances of schema registry running in the same cluster, the update request will be forwarded to one of the instances designated as the master.If the master is not available, the client will get an error code indicating that the forwarding has failed.
        /// </summary>
        public CompatibilityLevel GetGlobalConfig()
        {
            var compatibilityObject = Get<CompatibilityObject>($"/config");
            return ParseCompatibilityEnum(compatibilityObject);
        }

        /// <summary>
        /// Get global compatibility level.
        /// </summary>
        /// <param name="level">New global compatibility level.</param>
        public CompatibilityLevel PutGlobalConfig(CompatibilityLevel level)
        {
            var compatibilityObject = RunRequest<CompatibilityObject, CompatibilityObject>($"/config", HttpMethod.Put, CompatibilityObject.Create(level));
            return ParseCompatibilityEnum(compatibilityObject);
        }


        /// <summary>
        /// Get compatibility level for a subject.
        /// </summary>
        /// <param name="subject">Name of the subject</param>
        public CompatibilityLevel GetSubjectConfig(string subject)
        {
            var compatibilityObject = Get<CompatibilityObject>($"/config/{subject}");
            return ParseCompatibilityEnum(compatibilityObject);
        }

        /// <summary>
        /// Update compatibility level for the specified subject.
        /// </summary>
        /// <param name="subject">Name of the subject</param>
        /// <param name="level">New global compatibility level.</param>
        public CompatibilityLevel PutSubjectConfig(string subject, CompatibilityLevel level)
        {
            var compatibilityObject = RunRequest<CompatibilityObject, CompatibilityObject>($"/config/{subject}", HttpMethod.Put, CompatibilityObject.Create(level));
            return ParseCompatibilityEnum(compatibilityObject);
        }


        // Helpers 

        private static CompatibilityLevel ParseCompatibilityEnum(CompatibilityObject compatibilityObject)
        {
            if (compatibilityObject == null || compatibilityObject.Compatibility == null)
            {
                return CompatibilityLevel.NotSet;
            }

            return (CompatibilityLevel)Enum.Parse(typeof(CompatibilityLevel), compatibilityObject.Compatibility, true);
        }



        // IDisposible implementation

        private bool disposedValue = false;

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    _client.Dispose();
                }

                disposedValue = true;
            }
        }

        public void Dispose()
        {
            Dispose(true);
        }
    }
}
