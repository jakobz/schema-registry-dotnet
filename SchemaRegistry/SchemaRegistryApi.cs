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

        public SchemaRegistryApi(string url)
        {
            _registryUrl = url.TrimEnd('/');
        }

        // Generic API wrappers

        public async Task<TResponse> RunRequest<TResponse, TRequest>(string path, HttpMethod method, TRequest payload)
        {
            using (var httpClient = new HttpClient())
            {
                HttpRequestMessage request = new HttpRequestMessage();
                request.Headers.Add("Accept", "application/vnd.schemaregistry.v1+json, application/vnd.schemaregistry+json, application/json");
                request.Method = method;
                var url = _registryUrl + path;
                request.RequestUri = new Uri(url);

                if (payload != null)
                {
                    var payloadString = JsonUtils.ToJson(payload);
                    request.Headers.Add("Content-Type", "application/json");
                    request.Content = new StringContent(payloadString, Encoding.UTF8);
                }

                var response = await httpClient.SendAsync(request);
                var responseString = await response.Content.ReadAsStringAsync();

                if (response.StatusCode != HttpStatusCode.OK)
                {
                    SchemaRegistryError error = null;
                    try
                    {
                        error = JsonUtils.FromJson<SchemaRegistryError>(responseString);
                    }
                    catch
                    {
                    }

                    throw new SchemaRegistryException(url, response.StatusCode, error);
                }

                return JsonUtils.FromJson<TResponse>(responseString);
            }
        }


        public Task<TResponse> Get<TResponse>(string path)
        {
            return RunRequest<TResponse, string>(path, HttpMethod.Get, null);
        }


        // API Endpoints, as listed at http://docs.confluent.io/1.0.1/schema-registry/docs/api.html


        public Task<SchemaContainer> GetById(int id)
        {
            return Get<SchemaContainer>($"/schemas/ids/{id}");
        }


        public Task<string[]> GetAllSubjects()
        {
            return Get<string[]>("/subjects");
        }

        public Task<int[]> GetSchemaVersions(string subject)
        {
            return Get<int[]>($"/subjects/{subject}/versions");
        }

        public Task<SchemaMetadata> GetBySubjectAndId(string subject, int versionId)
        {
            return Get<SchemaMetadata>($"/subjects/{subject}/versions/{versionId}");
        }

        public Task<SchemaMetadata> GetLatestSchemaMetadata(string subject)
        {
            return Get<SchemaMetadata>($"/subjects/{subject}/versions/latest");
        }


        public async Task<int> Register(string subject, string schema)
        {
            var result = await RunRequest<IdContainer, SchemaContainer>($"/subjects/{subject}/versions", HttpMethod.Post, new SchemaContainer { Schema = schema });
            return result.Id;
        }


        public Task<ExistingSchemaResponse> CheckIfSchemaRegistered(string subject, string schema)
        {
            return RunRequest<ExistingSchemaResponse, SchemaContainer>($"/subjects/{subject}", HttpMethod.Post, new SchemaContainer { Schema = schema });
        }

        public Task<bool> TestCompatibilityWithVersion(string subject, int versionId, string schema)
        {
            return RunRequest<bool, SchemaContainer>($"/compatibility/subjects/{subject}/versions/{versionId}", HttpMethod.Post, new SchemaContainer { Schema = schema });
        }


        public async Task<bool> TestCompatibility(string subject, string schema)
        {
            var result = await RunRequest<CompatibilityFlagResponse, SchemaContainer>(
                $"/compatibility/subjects/{subject}/versions/latest",
                HttpMethod.Post,
                new SchemaContainer { Schema = schema });
                
            return result.IsCompatible;
        }

        public async Task<CompatibilityLevel> GetGlobalConfig()
        {
            var compatibilityObject = await Get<CompatibilityObject>($"/config");
            return ParseCompatibilityEnum(compatibilityObject);
        }

        public async Task<CompatibilityLevel> PutGlobalConfig(CompatibilityLevel level)
        {
            var compatibilityObject = await RunRequest<CompatibilityObject, CompatibilityObject>($"/config", HttpMethod.Put, CompatibilityObject.Create(level));
            return ParseCompatibilityEnum(compatibilityObject);
        }



        public async Task<CompatibilityLevel> GetSubjectConfig(string subject)
        {
            var compatibilityObject = await Get<CompatibilityObject>($"/config/{subject}");
            return ParseCompatibilityEnum(compatibilityObject);
        }


        public async Task<CompatibilityLevel> PutSubjectConfig(string subject, CompatibilityLevel level)
        {
            var compatibilityObject = await RunRequest<CompatibilityObject, CompatibilityObject>($"/config/{subject}", HttpMethod.Put, CompatibilityObject.Create(level));
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
