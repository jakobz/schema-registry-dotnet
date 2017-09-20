using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.IO;
using System.Linq;
using System.Net;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;

namespace SchemaRegistry
{
    /// <summary>
    /// Wrapper for AVRO serializer-deserializer, which works with schema registry.
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
        private string _topic;
        private ISchemaRegistryApi _registryApi;
        private bool _isKey;
        private string _subject;
        private ISerializerFactory<T> _serializerFactory;

        // caches
        private static ConcurrentDictionary<int, object> _deserializersCache
            = new ConcurrentDictionary<int, object>();

        private static ConcurrentDictionary<Type, Tuple<int, object>> _serializersCache 
            = new ConcurrentDictionary<Type, Tuple<int, object>>();

        public RegistryAwareSerializer(string topic, ISchemaRegistryApi registryApi, ISerializerFactory<T> serializerFactory, bool isKey = false)
        {
            _topic = topic;
            _registryApi = registryApi;
            _serializerFactory = serializerFactory;
            _isKey = isKey;
            _subject = topic + (isKey ? "-key" : "-value");
        }

        public T Deserialize(BinaryReader reader)
        {
            var magicByte = reader.ReadByte();
            if (magicByte != MagicByte)
            {
                throw new SerializationException("Magic byte is not found at the beginning og the schema-aware message. Make sure the message is in AVRO format, and is written with Confluent-compatible driver. Probably someone written JSON message in your AVRO topic?");
            }

            var schemaId = IPAddress.NetworkToHostOrder(reader.ReadInt32());

            var deserializer = (Func<Stream, T>)_deserializersCache.GetOrAdd(schemaId, _ =>
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
            var idAndSerializer = _serializersCache.GetOrAdd(typeof(T), type =>
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
