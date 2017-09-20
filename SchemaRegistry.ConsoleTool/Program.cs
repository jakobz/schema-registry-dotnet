using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SchemaRegistry.Utils;
using CLAP;
using System.IO;

namespace SchemaRegistry.ConsoleTool
{
    class Commands
    {
        [Empty, Help]
        public static void Help(string help)
        {
            Console.WriteLine(help);
        }

        [Verb(Description = "Updates subject compatibility")]
        public static void SetCompatibility([Description("Subject")] string subject, [Description("Level")] Messages.CompatibilityLevel level)
        {
            using (var registry = new SchemaRegistryApi("http://ecsc00104a5d.epam.com:8081/"))
            {
                Console.WriteLine($"Subject `{subject}` compatibility is now {registry.PutSubjectConfig(subject, level)}");
            }
        }

        [Verb(Description = "Updates subject compatibility")]
        public static void List(
            [Description("Schema registry URL")] string url
        )
        {
            using (var registry = new SchemaRegistryApi(url))
            {
                var subjects = registry.GetAllSubjects().Result;
                Console.WriteLine(String.Join(", ", subjects));
            }
        }
    }

    class Program
    {
        static void Main(string[] args)
        {
            Parser.RunConsole<Commands>(args);
        }
    }
}
