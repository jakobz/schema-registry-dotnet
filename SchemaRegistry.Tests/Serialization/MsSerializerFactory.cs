using SchemaRegistry.Serialization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SchemaRegistry.Tests.Serialization
{
    public class MsSerializerFactory<T> : ISerializerFactory<T>
    {
        public IDeserializer<T> BuildDeserializer(string writerSchema)
        {
            ///  AvroSerializer.CreateDeserializerOnly<T>(writerSchema, new AvroSerializerSettings())
            ///  

            return null;
        }

        public ISerializer<T> BuildSerializer()
        {
            //var newSerializer = AvroSerializer.Create<T>(new AvroSerializerSettings { GenerateDeserializer = false });

            //string hardcodedSchema = null;
            //var field = typeof(T).GetField("_SCHEMA", BindingFlags.Public | BindingFlags.Static);
            //if (field != null)
            //{
            //    hardcodedSchema = (string)field.GetValue(null);
            //}

            //var schema = hardcodedSchema ?? newSerializer.WriterSchema.ToString();

            //var newSchemaId = _registryApi.Register(_subject, schema);
            //return Tuple.Create(newSchemaId, (object)newSerializer);

            throw new NotImplementedException();
        }

        public string GetSchema()
        {
            //string hardcodedSchema = null;
            //var field = typeof(T).GetField("_SCHEMA", BindingFlags.Public | BindingFlags.Static);
            //if (field != null)
            //{
            //    hardcodedSchema = (string)field.GetValue(null);
            //}

            throw new NotImplementedException();
        }
    }
}
