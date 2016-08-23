using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SchemaRegistry.Serialization
{
    public interface ISerializer<T>
    {
        void Serialize(T obj, Stream stream);
    }
}
