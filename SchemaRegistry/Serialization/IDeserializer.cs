using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SchemaRegistry.Serialization
{
    public interface IDeserializer<T>
    {
        T Deserialize(Stream stream);
    }
}
