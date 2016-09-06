using System.Net.Http;
using SchemaRegistry.Messages;
using System;

namespace SchemaRegistry
{
    public interface ISchemaRegistryApi: IDisposable
    {
        ExistingSchemaResponse CheckIfSchemaRegistered(string subject, string schema);
        TResponse Get<TResponse>(string path);
        string[] GetAllSubjects();
        SchemaContainer GetById(int id);
        SchemaMetadata GetBySubjectAndId(string subject, int versionId);
        CompatibilityLevel GetGlobalConfig();
        SchemaMetadata GetLatestSchemaMetadata(string subject);
        int[] GetSchemaVersions(string subject);
        CompatibilityLevel GetSubjectConfig(string subject);
        CompatibilityLevel PutGlobalConfig(CompatibilityLevel level);
        CompatibilityLevel PutSubjectConfig(string subject, CompatibilityLevel level);
        int Register(string subject, string schema);
        TResponse RunRequest<TResponse, TRequest>(string path, HttpMethod method, TRequest payload);
        bool TestCompatibility(string subject, string schema);
        bool TestCompatibilityWithVersion(string subject, int versionId, string schema);
    }
}