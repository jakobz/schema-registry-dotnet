using Microsoft.Hadoop.Avro;
using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.IO;
using System.Linq;
using System.Net;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace SchemaRegistry
{
    public class KafkaAvroSerializer<T>
    {
        private const byte MagicByte = 0;
        private string _topic;
        private ISchemaRegistryApi _registryApi;
        private bool _isKey;
        private string _subject;

        // caches
        private static ConcurrentDictionary<int, object> _deserializersCache
            = new ConcurrentDictionary<int, object>();

        private static ConcurrentDictionary<Type, Tuple<int, object>> _serializersCache 
            = new ConcurrentDictionary<Type, Tuple<int, object>>();

        public KafkaAvroSerializer(string topic, ISchemaRegistryApi registryApi, bool isKey = false)
        {
            _topic = topic;
            _registryApi = registryApi;
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

            var serializer = (IAvroSerializer<T>)_deserializersCache.GetOrAdd(schemaId, _ =>
            {
                var writerSchema = _registryApi.GetBySubjectAndId(_subject, schemaId).Schema;
                var newSerializer = AvroSerializer.CreateDeserializerOnly<T>(writerSchema, new AvroSerializerSettings());
                return newSerializer;
            });

            var result = serializer.Deserialize(reader.BaseStream);

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
                var newSerializer = AvroSerializer.Create<T>(new AvroSerializerSettings { GenerateDeserializer = false });
                var newSchemaId = _registryApi.Register(_subject, newSerializer.WriterSchema.ToString());
                return Tuple.Create(newSchemaId, (object)newSerializer);
            });

            var schemaId = isAndSerializer.Item1;
            var serializer = (IAvroSerializer<T>)isAndSerializer.Item2;
            writer.Write(MagicByte);
            writer.Write(IPAddress.HostToNetworkOrder(schemaId));
            serializer.Serialize(writer.BaseStream, obj);
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
