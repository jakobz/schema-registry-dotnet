using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SchemaRegistry.Serialization
{
    public interface ISerializerFactory<T>
    {
        ISerializer<T> BuildSerializer();
        IDeserializer<T> BuildDeserializer(string writerSchema);
        string GetSchema();
    }
}
