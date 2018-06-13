using System;
using System.Threading.Tasks;
using SchemaRegistry.Messages;
using com.example.tests;
using System.Net.Http;

namespace SchemaRegistry.Tests
{
    public class MockSchemaRegistry : ISchemaRegistryApi
    {
        public Task<ExistingSchemaResponse> CheckIfSchemaRegistered(string subject, string schema)
        {
            throw new NotImplementedException();
        }

        public void Dispose()
        {
            throw new NotImplementedException();
        }

        public Task<TResponse> Get<TResponse>(string path)
        {
            throw new NotImplementedException();
        }

        public Task<string[]> GetAllSubjects()
        {
            throw new NotImplementedException();
        }

        public Task<SchemaContainer> GetById(int id)
        {
            return Task.FromResult(new SchemaContainer
            {
                Schema = User._SCHEMA.ToString()
            });
        }

        public Task<SchemaMetadata> GetBySubjectAndId(string subject, int versionId)
        {
            return Task.FromResult(new SchemaMetadata
            {
                Schema = User._SCHEMA.ToString(),
                Subject = subject,
                Version = 1
            });
        }

        public Task<CompatibilityLevel> GetGlobalConfig()
        {
            throw new NotImplementedException();
        }

        public Task<SchemaMetadata> GetLatestSchemaMetadata(string subject)
        {
            throw new NotImplementedException();
        }

        public Task<int[]> GetSchemaVersions(string subject)
        {
            throw new NotImplementedException();
        }

        public Task<CompatibilityLevel> GetSubjectConfig(string subject)
        {
            throw new NotImplementedException();
        }

        public Task<TResponse> Post<TResponse>(string path)
        {
            throw new NotImplementedException();
        }

        public Task<CompatibilityLevel> PutGlobalConfig(CompatibilityLevel level)
        {
            throw new NotImplementedException();
        }

        public Task<CompatibilityLevel> PutSubjectConfig(string subject, CompatibilityLevel level)
        {
            throw new NotImplementedException();
        }

        public Task<int> Register(string subject, string schema)
        {
            return Task.FromResult(3);
        }

        public Task<TResponse> RunRequest<TResponse, TRequest>(string path, HttpMethod method, TRequest payload)
        {
            throw new NotImplementedException();
        }

        public Task<bool> TestCompatibility(string subject, string schema)
        {
            throw new NotImplementedException();
        }

        public Task<bool> TestCompatibilityWithVersion(string subject, int versionId, string schema)
        {
            throw new NotImplementedException();
        }
    }
}
