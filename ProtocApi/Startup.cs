using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Funq;
using Microsoft.Extensions.Hosting;
using ServiceStack;
using ProtocApi.ServiceInterface;
using ProtocApi.ServiceModel;
using ServiceStack.Script;
using ServiceStack.Text;

namespace ProtocApi
{
    public class Startup : ModularStartup
    {
        // This method gets called by the runtime. Use this method to add services to the container.
        // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
        public new void ConfigureServices(IServiceCollection services)
        {
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseServiceStack(new AppHost
            {
                AppSettings = new NetCoreAppSettings(Configuration)
            });
        }
    }

    public class AppHost : AppHostBase
    {
        public AppHost() : base("ProtocApi", typeof(ProtocServices).Assembly) { }

        // Configure your AppHost with the necessary configuration and dependencies your App needs
        public override void Configure(Container container)
        {
            SetConfig(new HostConfig
            {
                DebugMode = AppSettings.Get(nameof(HostConfig.DebugMode), false)
            });
            
            var protocPath = Path.Combine(ContentRootDirectory.RealPath, "protoc");
            var protocConfig = new ProtocConfig {
                ExeName = Env.IsWindows
                    ? "protoc.exe"
                    : "protoc",
                WorkingDirectory = Env.IsWindows
                    ? Path.Combine(protocPath, "win64")
                    : Path.Combine(protocPath, "linux64"),
                ProtoIncludeDirectory = Path.Combine(protocPath, "include"),
                TempDirectory = Path.Combine(ContentRootDirectory.RealPath, "tmp"),
            };
            container.Register(protocConfig);

            if (Env.IsWindows)
                protocConfig.Languages.Remove(Lang.Swift); // protoc-gen-grpc-swift does not exist on Windows

            Plugins.Add(new SharpPagesFeature {
                ScriptMethods = { new ProtocScriptMethods(protocConfig) }
            });
        }
    }

    public class ProtocScriptMethods : ScriptMethods
    {
        private readonly ProtocConfig config;
        public ProtocScriptMethods(ProtocConfig config) => this.config = config;

        private List<KeyValuePair<string, string>> _langs;
        public List<KeyValuePair<string, string>> langs() => _langs ?? (_langs = config.Languages
            .Map(x => KeyValuePair.Create(x.Key.ToJsv(), x.Value.Name)).OrderBy(x => x.Key).ToList());
    }
}
