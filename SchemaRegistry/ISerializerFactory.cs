using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SchemaRegistry
{
    public interface ISerializerFactory<T>
    {
        Action<Stream, T> BuildSerializer();
        Func<Stream, T> BuildDeserializer(string writerSchema);
        string GetSchema();
    }
}
