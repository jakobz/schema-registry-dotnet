using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SchemaRegistry.Messages
{
    public class SchemaMetadata: SchemaContainer
    {
        public string Subject { get; set; }
        public int Version { get; set; }
    }
}
