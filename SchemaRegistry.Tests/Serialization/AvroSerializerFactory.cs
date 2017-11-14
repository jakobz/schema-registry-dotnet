using Avro.Specific;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using Avro;
using Avro.IO;

namespace SchemaRegistry.Tests.Serialization
{
    public class AvroSerializerFactory<T> : ISerializerFactory<T>
    {
        private Schema _schema;
        private string _schemaJson;

        public AvroSerializerFactory()
        {
            if (typeof(T) == typeof(byte[]))
            {
                _schema = PrimitiveSchema.NewInstance("bytes");
            }
            else if (typeof(T) == typeof(int))
            {
                _schema = PrimitiveSchema.NewInstance("int");
            }
            else if (typeof(T) == typeof(long))
            {
                _schema = PrimitiveSchema.NewInstance("long");
            }
            else if (typeof(T).IsPrimitive || typeof(T) == typeof(string))
            {
                _schema = PrimitiveSchema.NewInstance(typeof(T).Name.ToLowerInvariant());
            }
            else if (typeof(T).IsClass)
            {
                var schemaField = typeof(T).GetField("_SCHEMA", BindingFlags.Public | BindingFlags.Static);

                if (schemaField != null)
                {
                    _schema = (Schema)typeof(T).GetField("_SCHEMA", BindingFlags.Public | BindingFlags.Static).GetValue(null);
                }
            }

            if (_schema == null)
            {
                throw new ApplicationException($"Can't decide AVRO schema for type {typeof(T).FullName}");
            }

            _schemaJson = _schema.ToString();
        }

        public Func<Stream, T> BuildDeserializer(string writerSchemaJson)
        {
            var writerSchema = Schema.Parse(writerSchemaJson);
            var avroReader = new SpecificReader<T>(writerSchema, _schema);

            return (Stream stream) =>
            {
                var result = default(T);
                var decoder = new BinaryDecoder(stream);
                result = avroReader.Read(result, decoder);
                return result;
            };
        }

        public Action<Stream, T> BuildSerializer()
        {
            var avroWriter = new SpecificWriter<T>(_schema);

            return (Stream stream, T obj) =>
            {
                var encoder = new BinaryEncoder(stream);
                avroWriter.Write(obj, encoder);
            };
        }

        public string GetSchema()
        {
            return _schemaJson;
        }
    }
}
