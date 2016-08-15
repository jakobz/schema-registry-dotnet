using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace SchemaRegistry.Messages
{
    public class SchemaRegistryException: Exception
    {
        public SchemaRegistryErrorCode ErrorCode { get; set; }

        public SchemaRegistryException(SchemaRegistryError error, WebException webException): base(error?.Message ?? webException.Message, webException)
        {
            if (error != null)
            {
                ErrorCode = (SchemaRegistryErrorCode)error.ErrorCode;
            }
        }
    }
}
