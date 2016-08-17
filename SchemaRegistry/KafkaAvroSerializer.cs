using Microsoft.Hadoop.Avro;
using System;
using System.Collections.Generic;
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

        public KafkaAvroSerializer(string topic, ISchemaRegistryApi registryApi, bool isKey = false)
        {
            _topic = topic;
            _registryApi = registryApi;
            _isKey = isKey;
            _subject = topic + (isKey ? "-key" : "-value");
        }

        public void Serialize(T value, Stream stream)
        {

        }

        public T Deserialize(BinaryReader reader)
        {
            var magicByte = reader.ReadByte();
            if (magicByte != MagicByte)
            {
                throw new SerializationException("Magic byte is not found in the schema-aware message");
            }

            var schemaId = IPAddress.NetworkToHostOrder(reader.ReadInt32());
            var schema = _registryApi.GetBySubjectAndId(_subject, schemaId).Schema;

            //var serializer = AvroSerializer.Create<T>();
            //var result = serializer.Deserialize(reader.BaseStream);

            //AvroSerializer.CreateDeserializerOnly<T>(schema, new AvroSerializerSettings());

            var serializer = AvroSerializer.CreateGeneric(schema);
            dynamic obj = serializer.Deserialize(reader.BaseStream);

            return default(T);
        }
    }
}
