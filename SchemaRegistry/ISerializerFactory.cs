using System;
using System.IO;

namespace SchemaRegistry
{
    public interface ISerializerFactory<T>
    {
        Action<Stream, T> BuildSerializer();
        Func<Stream, T> BuildDeserializer(string writerSchema);
        string GetSchema();
    }
}
