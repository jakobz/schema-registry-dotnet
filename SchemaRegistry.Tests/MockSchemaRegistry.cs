using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using SchemaRegistry.Messages;
using example.avro;

namespace SchemaRegistry.Tests
{
    public class MockSchemaRegistry : ISchemaRegistryApi
    {        
        public ExistingSchemaResponse CheckIfSchemaRegistered(string subject, string schema)
        {
            throw new NotImplementedException();
        }

        public void Dispose()
        {
            throw new NotImplementedException();
        }

        public TResponse Get<TResponse>(string path)
        {
            throw new NotImplementedException();
        }

        public string[] GetAllSubjects()
        {
            throw new NotImplementedException();
        }

        public SchemaContainer GetById(int id)
        {
            return new SchemaContainer
            {
                Schema = User._SCHEMA.ToString()
            };
        }

        public SchemaMetadata GetBySubjectAndId(string subject, int versionId)
        {
            return new SchemaMetadata
            {
                Schema = User._SCHEMA.ToString(),
                Subject = subject,
                Version = 1
            };
        }

        public CompatibilityLevel GetGlobalConfig()
        {
            throw new NotImplementedException();
        }

        public SchemaMetadata GetLatestSchemaMetadata(string subject)
        {
            throw new NotImplementedException();
        }

        public int[] GetSchemaVersions(string subject)
        {
            throw new NotImplementedException();
        }

        public CompatibilityLevel GetSubjectConfig(string subject)
        {
            throw new NotImplementedException();
        }

        public TResponse Post<TResponse>(string path)
        {
            throw new NotImplementedException();
        }

        public CompatibilityLevel PutGlobalConfig(CompatibilityLevel level)
        {
            throw new NotImplementedException();
        }

        public CompatibilityLevel PutSubjectConfig(string subject, CompatibilityLevel level)
        {
            throw new NotImplementedException();
        }

        public int Register(string subject, string schema)
        {
            return 3;
        }

        public TResponse RunRequest<TResponse, TRequest>(string path, HttpMethod method, TRequest payload)
        {
            throw new NotImplementedException();
        }

        public bool TestCompatibility(string subject, string schema)
        {
            throw new NotImplementedException();
        }

        public bool TestCompatibilityWithVersion(string subject, int versionId, string schema)
        {
            throw new NotImplementedException();
        }
    }
}
