using com.example.tests;
using NUnit.Framework;
using SchemaRegistry.Tests.Serialization;
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
        byte[] TestMessage = new byte[] { 0, 0, 0, 0, 3, 26, 78, 97, 109, 101, 56,
            49, 51, 49, 51, 52, 52, 53, 55, 2, 242, 201, 187, 135, 6, 2, 28, 67, 111,
            108, 111, 114, 56, 49, 51, 49, 51, 52, 52, 53, 55 };

        User TestUser = new User
        {
            name = "Name813134457",
            favorite_color = "Color813134457",
            favorite_number = 813134457
        };

        public RegistryAwareSerializer<User> GetSerializer()
        {
            var avroSerializerFactory = new AvroSerializerFactory<User>();
            return new RegistryAwareSerializer<User>("test_topic", new MockSchemaRegistry(), avroSerializerFactory, false);
        }

        [Test]
        public void CanDeserialize()
        {
            var serializer = GetSerializer();
            var ms = new MemoryStream();
            ms.Write(TestMessage, 0, TestMessage.Length);
            ms.Position = 0;
            var msg = serializer.Deserialize(new BinaryReader(ms));
            Assert.AreEqual(TestUser.name, msg.name);
            Assert.AreEqual(TestUser.favorite_color, msg.favorite_color);
            Assert.AreEqual(TestUser.favorite_number, msg.favorite_number);
        }

        [Test]
        public void CanSerializeAndDeserializeBack()
        {
            var serializer = GetSerializer();

            var ms = new MemoryStream();
            
            using (var writer = new BinaryWriter(ms, Encoding.UTF8, true))
            {
                serializer.Serialize(TestUser, writer);
            }

            ms.Position = 0;

            using (var reader = new BinaryReader(ms))
            {
                var msg = serializer.Deserialize(reader);
                Assert.AreEqual(TestUser.name, msg.name);
                Assert.AreEqual(TestUser.favorite_color, msg.favorite_color);
                Assert.AreEqual(TestUser.favorite_number, msg.favorite_number);
            }
        }

        [Test]
        public void CanSerialize()
        {
            var serializer = GetSerializer();

            var ms = new MemoryStream();

            using (var writer = new BinaryWriter(ms))
            {
                serializer.Serialize(TestUser, writer);
                var bytes = ms.ToArray();
            }
        }
    }
}
