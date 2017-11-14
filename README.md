# schema-registry-dotnet

<!---
[![Build status](https://ci.appveyor.com/api/projects/status/iw1d99tqhchpdtks/branch/master?svg=true)](https://ci.appveyor.com/project/jakobz/schema-registry-dotnet/branch/master)
-->
.Net wrapper for the [Confluent Schema Registry](http://docs.confluent.io/1.0.1/schema-registry/docs/index.html) REST API.

REST API usage:
```c#
using (var registry = new SchemaRegistryApi("http://schema-registry.my-company.com"))
{
    // Get first 10 subjects
    var subjects = registry.GetAllSubjects().Result.Take(10);
    Console.WriteLine("First 10 subjects: " + String.Join(", ", subjects));

    // Get last schema by subject
    var subject = subjects.First();
    var meta = registry.GetLatestSchemaMetadata(subject).Result;
    Console.WriteLine($"Last version of the {subject} subject: {meta.Version}");

    // Check schema compatibiliy
    var isCompatible = registry.TestCompatibility(subject, meta.Schema).Result;
    Console.WriteLine($"Is the schema compatible to itself: {isCompatible}");

    // Register the schema (returns the same ID for identical schema)
    var newSchemaId = registry.Register(subject, meta.Schema).Result;
    Console.WriteLine($"Schema id: {newSchemaId}");
}
```

There's also RegistryAwareSerializer<T> class, which implements Confluent's AVRO+Schema registry convention.
  
On serialize: 
- Access Schema Registry to check Schema compatibility and get Schema ID (statically cached)
- Write prefix containing the 0 magic byte and the schema ID to the message

On deserialize:
- Reads message prefix to check magic 0 byte and extract Schema ID
- Access Schema Registry to get Schema
- Provides retrieved schema as Writer Schema to the serializerConcrete serializer implementation is pluggable via serializerFactory constructor argument.

AVRO serializer implementation is pluggable

Example implementation for [Apache AVRO library](https://www.nuget.org/packages/Apache.Avro/) is [included in tests](https://github.com/jakobz/schema-registry-dotnet/blob/master/SchemaRegistry.Tests/Serialization/AvroSerializerFactory.cs)
