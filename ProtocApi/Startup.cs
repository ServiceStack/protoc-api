using System.IO;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Funq;
using ServiceStack;
using ServiceStack.Configuration;
using ProtocApi.ServiceInterface;
using ServiceStack.Text;

namespace ProtocApi
{
    public class Startup : ModularStartup
    {
        public Startup(IConfiguration configuration) : base(configuration){}

        // This method gets called by the runtime. Use this method to add services to the container.
        // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
        public new void ConfigureServices(IServiceCollection services)
        {
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
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
                DefaultRedirectPath = "/metadata",
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
        }
    }
}
