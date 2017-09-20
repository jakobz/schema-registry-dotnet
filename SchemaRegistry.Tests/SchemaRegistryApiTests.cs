using NUnit.Framework;
using SchemaRegistry.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework.Constraints;
using SchemaRegistry.Messages;

namespace SchemaRegistry.Tests
{
    [TestFixture]
    [Ignore("Integration tests")]
    public class SchemaRegistryApiTests
    {
        [Test]
        public async void CanReadSchemas()
        {
            using (var registry = TestsConfig.GetRegistryApi())
            {
                var subjects = await registry.GetAllSubjects();
                Console.WriteLine(subjects.ToJson());

                var versions = await registry.GetSchemaVersions(subjects[0]);
                Console.WriteLine(versions.ToJson());

                var schema = await registry.GetById(versions[0]);
                Console.WriteLine(schema.Schema);

                var schemaVersionSpecific = await registry.GetBySubjectAndId(subjects[0], versions[0]);
                Console.WriteLine(schemaVersionSpecific.ToJson());

                var schemaVersionLatest = await registry.GetLatestSchemaMetadata(subjects[0]);
                Console.WriteLine(schemaVersionLatest.ToJson());
            }
        }

        [Test]
        public async void CanCheckIfSchemaRegistered()
        {
            using (var registry = TestsConfig.GetRegistryApi())
            {
                var subject = (await registry.GetAllSubjects())[0];
                var schema = (await registry.GetLatestSchemaMetadata(subject)).Schema;
                var existing = await registry.CheckIfSchemaRegistered(subject, schema);
                Assert.IsNotNull(existing);
                Assert.AreEqual(schema, existing.Schema);
                Assert.AreNotEqual(existing.Id, 0);
                Assert.AreEqual(subject, existing.Subject);
                Assert.IsTrue((await registry.TestCompatibility(subject, schema)));

            }
        }

        [Test]
        public async void CanReadConfig()
        {
            using (var registry = TestsConfig.GetRegistryApi())
            {
                await registry.GetGlobalConfig();
                var subject = (await registry.GetAllSubjects())[0];
                await registry.GetSubjectConfig(subject);
            }
        }

        string InitialSchema = "{'type':'record','name':'myrecord','fields':[{'name':'f1','type':'string'}]}".Replace("'", "\"");
        string FullyCompatibleSchema = "{'type':'record','name':'myrecord','fields':[{'name':'f1','type':'string'},{'name':'f2','type':'string','default': 'none'}]}".Replace("'", "\"");
        string BackwardsIncompatibleSchema = "{'type':'record','name':'myrecord','fields':[{'name':'f1','type':'string'},{'name':'f2','type':'string'}]}".Replace("'", "\"");
        string ForwardIncompatibleSchema = "{'type':'record','name':'myrecord','fields':[{'name':'f2','type':'string'}]}".Replace("'", "\"");
        string ForwardCompatibleSchema = "{'type':'record','name':'myrecord','fields':[{'name':'f1','type':'string','default':'none'},{'name':'f2','type':'string'}]}".Replace("'", "\"");
        string CompletelyIncompatibleSchema = "{'type':'record','name':'myrecord','fields':[{'name':'newf','type':'string'}]}".Replace("'", "\"");

        [Test]
        public async void CanTestCompatibility()
        {
            using (var registry = TestsConfig.GetRegistryApi())
            {
                var subject = (await registry.GetAllSubjects())[0];
                var schema = (await registry.GetLatestSchemaMetadata(subject)).Schema;
                await registry.PutSubjectConfig(subject, CompatibilityLevel.Backward);
                Assert.IsTrue(await registry.TestCompatibility(subject, schema));
                Assert.IsTrue(await registry.TestCompatibility(subject, FullyCompatibleSchema));
                Assert.IsFalse(await registry.TestCompatibility(subject, BackwardsIncompatibleSchema));
                Assert.IsFalse(await registry.TestCompatibility(subject, CompletelyIncompatibleSchema));
                await registry.PutSubjectConfig(subject, CompatibilityLevel.Forward);
                Assert.IsTrue(await registry.TestCompatibility(subject, schema));
                Assert.IsTrue(await registry.TestCompatibility(subject, ForwardCompatibleSchema));
                Assert.IsFalse(await registry.TestCompatibility(subject, ForwardIncompatibleSchema));
                Assert.IsFalse(await registry.TestCompatibility(subject, CompletelyIncompatibleSchema));

                await registry.PutSubjectConfig(subject, CompatibilityLevel.Full);
                Assert.IsTrue(await registry.TestCompatibility(subject, schema));
                Assert.IsTrue(await registry.TestCompatibility(subject, FullyCompatibleSchema));
                Assert.IsFalse(await registry.TestCompatibility(subject, BackwardsIncompatibleSchema));
                Assert.IsFalse(await registry.TestCompatibility(subject, ForwardCompatibleSchema));
                Assert.IsFalse(await registry.TestCompatibility(subject, ForwardIncompatibleSchema));
                Assert.IsFalse(await registry.TestCompatibility(subject, CompletelyIncompatibleSchema));

                await registry.PutSubjectConfig(subject, CompatibilityLevel.Forward);                
            }
        }

        [Test]
        public void CanRegisterSchema()
        {
            using (var registry = TestsConfig.GetRegistryApi())
            {
                var subject = "dotnet-schema-registry-api-test2";

                registry.PutSubjectConfig(subject, CompatibilityLevel.Backward);

                var response1 = registry.Register(subject, InitialSchema);
                //var code = Assert.Throws<SchemaRegistryException>(() => registry.Register(subject, InitialSchema));
                //Assert.AreEqual(code, SchemaRegistryErrorCode.IncompatibleAvroSchema);
                var response2 = registry.Register(subject, FullyCompatibleSchema);

                var code = Assert.Throws<SchemaRegistryException>(() => registry.Register(subject, CompletelyIncompatibleSchema));
            }
        }

        private IResolveConstraint SchemaRegistryException()
        {
            throw new NotImplementedException();
        }
    }
}
