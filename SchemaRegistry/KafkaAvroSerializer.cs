using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.IO;
using System.Linq;
using System.Net;
using System.Runtime.Serialization;
using System.Threading.Tasks;
using System.Reflection;
using Avro;
using Avro.Generic;
using Avro.Specific;
using Avro.IO;

namespace SchemaRegistry
{
    public class KafkaAvroSerializer<T> where T : ISpecificRecord, new()
    {
        private const byte MagicByte = 0;
        private string _topic;
        private ISchemaRegistryApi _registryApi;
        private bool _isKey;
        private string _subject;
        private Schema _schema;

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

            _schema = (Schema)typeof(T).GetField("_SCHEMA", BindingFlags.Public | BindingFlags.Static).GetValue(null);
        }

        public T Deserialize(BinaryReader reader)
        {
            var magicByte = reader.ReadByte();
            if (magicByte != MagicByte)
            {
                throw new SerializationException("Magic byte is not found in the schema-aware message");
            }

            var schemaId = IPAddress.NetworkToHostOrder(reader.ReadInt32());

            var serializer = (SpecificReader<T>)_deserializersCache.GetOrAdd(schemaId, _ =>
            {
                var writerSchemaJson = _registryApi.GetById(schemaId).Schema;
                var writerSchema = Schema.Parse(writerSchemaJson);
                var avroReader = new SpecificReader<T>(writerSchema, _schema);
                return avroReader;
            });

            var result = default(T);
            var decoder = new BinaryDecoder(reader.BaseStream);
            result = serializer.Read(result, decoder);

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
                var newSchemaId = _registryApi.Register(_subject, _schema.ToString());
                var avroWriter = new SpecificWriter<T>(_schema);
                return Tuple.Create(newSchemaId, (object)avroWriter);
            });

            var schemaId = idAndSerializer.Item1;
            var serializer = (SpecificWriter<T>)idAndSerializer.Item2;
            writer.Write(MagicByte);
            writer.Write(IPAddress.HostToNetworkOrder(schemaId));
            var encoder = new BinaryEncoder(writer.BaseStream);
            serializer.Write(obj, encoder);
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
