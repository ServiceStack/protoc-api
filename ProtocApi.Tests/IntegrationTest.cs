using System.Collections.Generic;
using System.IO;
using Funq;
using ServiceStack;
using NUnit.Framework;
using ProtocApi.ServiceInterface;
using ProtocApi.ServiceModel;
using ServiceStack.IO;
using ServiceStack.Logging;
using ServiceStack.Text;

namespace ProtocApi.Tests
{
    [Explicit]
    public partial class IntegrationTest
    {
        const string BaseUri = "http://localhost:2000/";
        private readonly ServiceStackHost appHost;

        class AppHost : AppSelfHostBase
        {
            public AppHost() : base(nameof(IntegrationTest), typeof(ProtocServices).Assembly) { }

            public override void Configure(Container container)
            {
                SetConfig(new HostConfig {
                    DebugMode = true,
                });
                
                var contentRoot = Path.GetFullPath("../../../../ProtocApi");
                var protocPath = Path.Combine(contentRoot, "protoc");
                var exeDirPath = Env.IsWindows
                    ? Path.Combine(protocPath, "win64")
                    : Path.Combine(protocPath, "linux64");
                var protocConfig = new ProtocConfig {
                    ExePath = Env.IsWindows
                        ? Path.Combine(exeDirPath, "protoc.exe")
                        : Path.Combine(exeDirPath, "protoc"),
                    PluginPath = exeDirPath,
                    ProtoIncludeDirectory = Path.Combine(protocPath, "include"),
                    TempDirectory = Path.Combine(contentRoot, "tmp"),
                };
                
                try { FileSystemVirtualFiles.DeleteDirectory(protocConfig.TempDirectory); } catch {}
                try { Directory.CreateDirectory(protocConfig.TempDirectory); } catch {}
                
                container.Register(protocConfig);
            }
        }

        public IntegrationTest()
        {
            LogManager.LogFactory = new StringBuilderLogFactory();
            
            appHost = new AppHost()
                .Init()
                .Start(BaseUri);
        }

        [OneTimeTearDown]
        public void OneTimeTearDown() => appHost.Dispose();

        private static void PrintLogs()
        {
            if (LogManager.LogFactory is StringBuilderLogFactory sbLogs)
            {
                var logs = sbLogs.GetLogs();
                logs.Print();
            }
        }

        public IServiceClient CreateClient() => new JsonServiceClient(BaseUri);

        [Test]
        public void Can_call_Hello_Service()
        {
            var client = CreateClient();

            var response = client.Get(new Hello { Name = "World" });

            Assert.That(response.Result, Is.EqualTo("Hello, World!"));
        }

    }
}