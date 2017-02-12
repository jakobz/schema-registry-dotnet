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
                throw new SerializationException("Magic byte is not found in the schema-aware message");
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
            var isAndSerializer = _serializersCache.GetOrAdd(typeof(T), type =>
            {
                var newSerializer = _serializerFactory.BuildSerializer();
                var schema = _serializerFactory.GetSchema();
                var newSchemaId = _registryApi.Register(_subject, schema).Result;
                return Tuple.Create(newSchemaId, (object)newSerializer);
            });

            var schemaId = isAndSerializer.Item1;
            var serializer = (Action<Stream, T>)isAndSerializer.Item2;
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
