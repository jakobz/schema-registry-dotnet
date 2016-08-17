using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace SchemaRegistry.Tests
{
    [TestFixture]
    public class KafkaSerializerTests
    {
        byte[] TestMessage = new byte[] { 0, 0, 0, 0, 3, 26, 78, 97, 109, 101, 56, 49,
            51, 49, 51, 52, 52, 53, 55, 0, 242, 201, 187, 135, 6, 0, 28, 67, 111, 108,
            111, 114, 56, 49, 51, 49, 51, 52, 52, 53, 55 };

        //[DataContract(Name = "user", Namespace = "com.epam.avro")]
        //private class User
        //{
        //    [DataMember(Name = "name")]
        //    public string Name { get; set; }
        //    [DataMember(Name = "favorite_number")]
        //    public int? Value { get; set; }
        //    [DataMember(Name = "favorite_color")]
        //    public string FavoriteColor { get; set; }
        //}

        [Test]
        public void CanDeserialize()
        {
            var serializer = new KafkaAvroSerializer<example.avro.User>("test_topic", new MockSchemaRegistry(), false);
            var msg = serializer.Deserialize(new BinaryReader(new MemoryStream(TestMessage)));
            Assert.AreEqual(msg.name, "test");
        }
    }
}
