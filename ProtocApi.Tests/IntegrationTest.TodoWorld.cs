using System.Collections.Generic;
using System.IO;
using NUnit.Framework;
using ProtocApi.ServiceModel;

namespace ProtocApi.Tests
{
    public partial class IntegrationTest
    {
        private static Dictionary<string, string> GetTodoWorldFiles() => 
            new Dictionary<string, string> {
                ["services.proto"] = File.ReadAllText("protos/todoworld/services.proto")
            };
        
        static List<string> TodoWorldTypeNames = new List<string> {
            "Todo",
            "GetTodo",
            "GetTodoResponse",
            "GetTodos",
            "GetTodosResponse",
            "CreateTodo",
            "CreateTodoResponse",
            "Authenticate",
            "AuthenticateResponse",
            "ResponseStatus",
            "ResponseError",
            "StreamFiles",
            "StreamServerEvents",
            "StreamServerEventsResponse",
        };
        
        [Test]
        public void Can_todoworld_protoc_csharp()
        {
            var client = CreateClient();

            var response = client.Post(new Protoc {
                Lang = Lang.CSharp,
                Files = GetTodoWorldFiles(),
            });
            
            Assert.That(response.GeneratedFiles["Services.cs"], Is.Not.Empty);
        }

        [Test]
        public void Can_todoworld_protoc_cpp()
        {
            var client = CreateClient();

            var response = client.Post(new Protoc {
                Lang = Lang.Cpp,
                Files = GetTodoWorldFiles(),
            });
            
            Assert.That(response.GeneratedFiles["services.pb.h"], Is.Not.Empty);
            Assert.That(response.GeneratedFiles["services.pb.cc"], Is.Not.Empty);
        }

        [Test]
        public void Can_todoworld_protoc_javascript_closure()
        {
            var client = CreateClient();

            var response = client.Post(new Protoc {
                Lang = Lang.JavaScriptClosure,
                Files = GetTodoWorldFiles(),
            });
            
            TodoWorldTypeNames.ForEach(x => Assert.That(response.GeneratedFiles[x.ToLower() + ".js"], Is.Not.Empty));
        }
        
        [Test]
        public void Can_todoworld_protoc_javascript_commonjs()
        {
            var client = CreateClient();

            var response = client.Post(new Protoc {
                Lang = Lang.JavaScriptCommonJs,
                Files = GetTodoWorldFiles(),
            });
            
            Assert.That(response.GeneratedFiles["services_pb.js"], Is.Not.Empty);
            Assert.That(response.GeneratedFiles["services_grpc_web_pb.js"], Is.Not.Empty);
        }
        
        [Test]
        public void Can_todoworld_protoc_javascript_typescript()
        {
            var client = CreateClient();

            var response = client.Post(new Protoc {
                Lang = Lang.TypeScript,
                Files = GetTodoWorldFiles(),
            });
            
            Assert.That(response.GeneratedFiles["services_pb.d.ts"], Is.Not.Empty);
            Assert.That(response.GeneratedFiles["services_pb.js"], Is.Not.Empty);
            Assert.That(response.GeneratedFiles["ServicesServiceClientPb.ts"], Is.Not.Empty);
        }
        
        [Test]
        public void Can_todoworld_protoc_javascript_typescript_binary()
        {
            var client = CreateClient();

            var response = client.Post(new Protoc {
                Lang = Lang.TypeScriptBinary,
                Files = GetTodoWorldFiles(),
            });
            
            Assert.That(response.GeneratedFiles["services_pb.d.ts"], Is.Not.Empty);
            Assert.That(response.GeneratedFiles["services_pb.js"], Is.Not.Empty);
            Assert.That(response.GeneratedFiles["ServicesServiceClientPb.ts"], Is.Not.Empty);
        }
 
        [Test]
        public void Can_todoworld_protoc_python()
        {
            var client = CreateClient();

            var response = client.Post(new Protoc {
                Lang = Lang.Python,
                Files = GetTodoWorldFiles(),
            });
            
            Assert.That(response.GeneratedFiles["services_pb2.py"], Is.Not.Empty);
        }
 
        [Test]
        public void Can_todoworld_protoc_ruby()
        {
            var client = CreateClient();

            var response = client.Post(new Protoc {
                Lang = Lang.Ruby,
                Files = GetTodoWorldFiles(),
            });
            
            Assert.That(response.GeneratedFiles["services_pb.rb"], Is.Not.Empty);
        }

        [Test]
        public void Can_todoworld_protoc_php()
        {
            var client = CreateClient();

            var response = client.Post(new Protoc {
                Lang = Lang.Php,
                Files = GetTodoWorldFiles(),
            });
            
            TodoWorldTypeNames.ForEach(x => Assert.That(response.GeneratedFiles[x + ".php"], Is.Not.Empty));
        }

        [Test]
        public void Can_todoworld_protoc_java()
        {
            var client = CreateClient();

            var response = client.Post(new Protoc {
                Lang = Lang.Java,
                Files = GetTodoWorldFiles(),
            });
            
            Assert.That(response.GeneratedFiles["Services.java"], Is.Not.Empty);
        }

        [Test]
        public void Can_todoworld_protoc_javalite()
        {
            var client = CreateClient();

            var response = client.Post(new Protoc {
                Lang = Lang.JavaLite,
                Files = GetTodoWorldFiles(),
            });
            
            Assert.That(response.GeneratedFiles["Services.java"], Is.Not.Empty);
        }

        [Test]
        public void Can_todoworld_protoc_objectivec()
        {
            var client = CreateClient();

            var response = client.Post(new Protoc {
                Lang = Lang.ObjectiveC,
                Files = GetTodoWorldFiles(),
            });
            
            Assert.That(response.GeneratedFiles["Services.pbobjc.h"], Is.Not.Empty);
            Assert.That(response.GeneratedFiles["Services.pbobjc.m"], Is.Not.Empty);
        }

        [Test]
        public void Can_todoworld_protoc_go()
        {
            var client = CreateClient();

            var response = client.Post(new Protoc {
                Lang = Lang.Go,
                Files = GetTodoWorldFiles(),
            });
            
            Assert.That(response.GeneratedFiles["services.pb.go"], Is.Not.Empty);
        }

    }
}