using System;
using System.Net;

namespace SchemaRegistry.Messages
{
    public class SchemaRegistryException : Exception
    {
        public SchemaRegistryErrorCode ErrorCode { get; set; }

        public SchemaRegistryException(string url, HttpStatusCode status, SchemaRegistryError error)
            : base($"Schema registry returned error. Status: {status}.{(error == null ? "" : $" Error: {error.Message}")} Url: {url}")
        {
            if (error != null)
            {
                ErrorCode = (SchemaRegistryErrorCode)error.ErrorCode;
            }
        }
    }
}
