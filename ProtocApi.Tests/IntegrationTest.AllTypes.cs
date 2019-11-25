using System.Collections.Generic;
using System.IO;
using NUnit.Framework;
using ProtocApi.ServiceModel;
using ServiceStack.Text;

namespace ProtocApi.Tests
{
    public partial class IntegrationTest
    {
        private static Dictionary<string, string> GetAllTypesFiles() =>
            new Dictionary<string, string> {
                ["services.proto"] = File.ReadAllText("protos/alltypes/services.proto"),
                ["protobuf-net\\bcl.proto"] = File.ReadAllText("protos/alltypes/protobuf-net/bcl.proto"),
            };
        
        static List<string> AllTypesTypeNames = new List<string> {
            "AllCollectionTypes",
            "AllTypes",
            "DateTimeOffset",
            "FileContent",
            "Hello",
            "HelloAllTypes",
            "HelloAllTypesResponse",
            "HelloResponse",
            "KeyValuePair_String_String",
            "Poco",
            "Point",
            "ResponseError",
            "ResponseStatus",
            "StreamFiles",
            "StreamServerEvents",
            "StreamServerEventsResponse",
            "SubType",
        };

        [Test]
        public void Can_alltypes_protoc_csharp()
        {
            var client = CreateClient();

            var response = client.Post(new Protoc {
                Lang = Lang.CSharp,
                Files = GetAllTypesFiles()
            });
            
            Assert.That(response.GeneratedFiles["Services.cs"], Is.Not.Empty);
            Assert.That(response.GeneratedFiles["Bcl.cs"], Is.Not.Empty);
        }

        [Test]
        public void Can_protoc_cpp_alltypes()
        {
            var client = CreateClient();

            var response = client.Post(new Protoc {
                Lang = Lang.Cpp,
                Files = GetAllTypesFiles()
            });

            Assert.That(response.GeneratedFiles["services.pb.h"], Is.Not.Empty);
            Assert.That(response.GeneratedFiles["services.pb.cc"], Is.Not.Empty);
            Assert.That(response.GeneratedFiles["protobuf-net/bcl.pb.h"], Is.Not.Empty);
            Assert.That(response.GeneratedFiles["protobuf-net/bcl.pb.cc"], Is.Not.Empty);
        }

        [Test]
        public void Can_alltypes_protoc_javascript_closure()
        {
            var client = CreateClient();

            var response = client.Post(new Protoc {
                Lang = Lang.JavaScriptClosure,
                Files = GetAllTypesFiles(),
            });
            
            AllTypesTypeNames.ForEach(x => Assert.That(response.GeneratedFiles[x.ToLower() + ".js"], Is.Not.Empty));
        }
        
        [Test]
        public void Can_alltypes_protoc_javascript_commonjs()
        {
            var client = CreateClient();

            var response = client.Post(new Protoc {
                Lang = Lang.JavaScriptCommonJs,
                Files = GetAllTypesFiles(),
            });
            
            Assert.That(response.GeneratedFiles["services_pb.js"], Is.Not.Empty);
            Assert.That(response.GeneratedFiles["services_grpc_web_pb.js"], Is.Not.Empty);
        }
        
        [Test]
        public void Can_alltypes_protoc_javascript_typescript()
        {
            var client = CreateClient();

            var response = client.Post(new Protoc {
                Lang = Lang.TypeScript,
                Files = GetAllTypesFiles(),
            });
            
            Assert.That(response.GeneratedFiles["services_pb.d.ts"], Is.Not.Empty);
            Assert.That(response.GeneratedFiles["services_pb.js"], Is.Not.Empty);
            Assert.That(response.GeneratedFiles["ServicesServiceClientPb.ts"], Is.Not.Empty);
        }
        
        [Test]
        public void Can_alltypes_protoc_javascript_typescript_binary()
        {
            var client = CreateClient();

            var response = client.Post(new Protoc {
                Lang = Lang.TypeScriptBinary,
                Files = GetAllTypesFiles(),
            });
            
            Assert.That(response.GeneratedFiles["services_pb.d.ts"], Is.Not.Empty);
            Assert.That(response.GeneratedFiles["services_pb.js"], Is.Not.Empty);
            Assert.That(response.GeneratedFiles["ServicesServiceClientPb.ts"], Is.Not.Empty);
        }
 
        [Test]
        public void Can_alltypes_protoc_python()
        {
            var client = CreateClient();

            var response = client.Post(new Protoc {
                Lang = Lang.Python,
                Files = GetAllTypesFiles(),
            });
            
            Assert.That(response.GeneratedFiles["services_pb2.py"], Is.Not.Empty);
        }
 
        [Test]
        public void Can_alltypes_protoc_ruby()
        {
            var client = CreateClient();

            var response = client.Post(new Protoc {
                Lang = Lang.Ruby,
                Files = GetAllTypesFiles(),
            });
            
            Assert.That(response.GeneratedFiles["services_pb.rb"], Is.Not.Empty);
        }

        [Test]
        public void Can_alltypes_protoc_php()
        {
            var client = CreateClient();

            var response = client.Post(new Protoc {
                Lang = Lang.Php,
                Files = GetAllTypesFiles(),
            });
            
            AllTypesTypeNames.ForEach(x => Assert.That(response.GeneratedFiles[x + ".php"], Is.Not.Empty));
            Assert.That(response.GeneratedFiles["Bcl/DateTime.php"], Is.Not.Empty);
            Assert.That(response.GeneratedFiles["GPBMetadata/Services.php"], Is.Not.Empty);
            Assert.That(response.GeneratedFiles["GPBMetadata/ProtobufNet/Bcl.php"], Is.Not.Empty);
        }

        [Test]
        public void Can_alltypes_protoc_java()
        {
            var client = CreateClient();

            var response = client.Post(new Protoc {
                Lang = Lang.Java,
                Files = GetAllTypesFiles(),
            });
            
            Assert.That(response.GeneratedFiles["Services.java"], Is.Not.Empty);
        }

        [Test]
        public void Can_alltypes_protoc_javalite()
        {
            var client = CreateClient();

            var response = client.Post(new Protoc {
                Lang = Lang.JavaLite,
                Files = GetAllTypesFiles(),
            });
            
            Assert.That(response.GeneratedFiles["Services.java"], Is.Not.Empty);
        }

        [Test]
        public void Can_alltypes_protoc_objectivec()
        {
            var client = CreateClient();

            var response = client.Post(new Protoc {
                Lang = Lang.ObjectiveC,
                Files = GetAllTypesFiles(),
            });
            
            Assert.That(response.GeneratedFiles["Services.pbobjc.h"], Is.Not.Empty);
            Assert.That(response.GeneratedFiles["Services.pbobjc.m"], Is.Not.Empty);
        }

        [Test]
        public void Can_alltypes_protoc_go()
        {
            var client = CreateClient();

            var response = client.Post(new Protoc {
                Lang = Lang.Go,
                Files = GetAllTypesFiles(),
            });
            
            Assert.That(response.GeneratedFiles["services.pb.go"], Is.Not.Empty);
            Assert.That(response.GeneratedFiles["protobuf-net/bcl.pb.go"], Is.Not.Empty);
        }
    }
}