using System;
using System.Collections.Concurrent;
using System.IO;
using System.Net;
using System.Runtime.Serialization;

namespace SchemaRegistry
{
    /// <summary>
    /// Wrapper for AVRO serializer-deserializer, which implements Confluent's AVRO+Schema registry convention.
    /// Concrete serializer implementation is pluggable via serializerFactory constructor argument.
    /// On serialize:
    /// - Access Schema Registry to check Schema compatibility and get Schema ID (statically cached)
    /// - Write prefix containing schema ID to the message
    /// On deserialize:
    /// - Reads message prefix to extract Schema ID
    /// - Access Schema Registry to extract Schema
    /// - Provides retrieved schema as Writer Schema to the serializer
    /// </summary>
    /// <typeparam name="T">Message DTO</typeparam>
    public class RegistryAwareSerializer<T>
    {
        private const byte MagicByte = 0;
        private readonly ISchemaRegistryApi _registryApi;
        private readonly string _subject;
        private readonly ISerializerFactory<T> _serializerFactory;
        private readonly string _environment;

        // caches
        private static readonly ConcurrentDictionary<Tuple<int, string>, object> DeserializersCache
            = new ConcurrentDictionary<Tuple<int, string>, object>();

        private static readonly ConcurrentDictionary<Tuple<Type, string>, Tuple<int, object>> SerializersCache
            = new ConcurrentDictionary<Tuple<Type, string>, Tuple<int, object>>();

        public RegistryAwareSerializer(string topic, ISchemaRegistryApi registryApi, ISerializerFactory<T> serializerFactory, bool isKey = false, string environment = "Default")
        {
            _registryApi = registryApi;
            _serializerFactory = serializerFactory;
            _environment = environment;
            _subject = topic + (isKey ? "-key" : "-value");
        }

        public T Deserialize(BinaryReader reader)
        {
            var magicByte = reader.ReadByte();
            if (magicByte != MagicByte)
            {
                throw new SerializationException("Magic byte is not found at the beginning or the schema-aware message. Make sure the message is in AVRO format, and is written with Confluent-compatible driver. Probably someone written JSON message in your AVRO topic?");
            }

            var schemaId = IPAddress.NetworkToHostOrder(reader.ReadInt32());

            var deserializer = (Func<Stream, T>)DeserializersCache.GetOrAdd(Tuple.Create(schemaId, _environment), _ =>
            {
                var writerSchema = _registryApi.GetById(schemaId).Result.Schema;
                var newSerializer = _serializerFactory.BuildDeserializer(writerSchema);
                return newSerializer;
            });

            var result = deserializer(reader.BaseStream);

            return result;
        }

        public T Deserialize(byte[] bytes)
        {
            return Deserialize(new BinaryReader(new MemoryStream(bytes)));
        }

        public void Serialize(T obj, BinaryWriter writer)
        {
            var idAndSerializer = SerializersCache.GetOrAdd(Tuple.Create(typeof(T), _environment), type =>
            {
                var newSerializer = _serializerFactory.BuildSerializer();
                var schema = _serializerFactory.GetSchema();
                var newSchemaId = _registryApi.Register(_subject, schema).Result;
                return Tuple.Create(newSchemaId, (object)newSerializer);
            });

            var schemaId = idAndSerializer.Item1;
            var serializer = (Action<Stream, T>)idAndSerializer.Item2;
            writer.Write(MagicByte);
            writer.Write(IPAddress.HostToNetworkOrder(schemaId));
            serializer(writer.BaseStream, obj);
        }

        public byte[] SerializeToBytesArray(T obj)
        {
            using (var ms = new MemoryStream())
            using (var writer = new BinaryWriter(ms))
            {
                Serialize(obj, writer);
                writer.Flush();
                ms.Flush();
                return ms.ToArray();
            }
        }
    }
}
