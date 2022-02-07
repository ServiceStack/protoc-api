using Funq;
using ServiceStack;
using ProtocApi.ServiceInterface;
using ProtocApi.ServiceModel;
using ServiceStack.Script;
using ServiceStack.Text;

[assembly: HostingStartup(typeof(ProtocApi.AppHost))]

namespace ProtocApi;

public class AppHost : AppHostBase, IHostingStartup
{
    public void Configure(IWebHostBuilder builder) => builder
        .ConfigureServices(services => {
            // Configure ASP.NET Core IOC Dependencies
        })
        .Configure(app => {
            // Configure ASP.NET Core App
            if (!HasInit)
                app.UseServiceStack(new AppHost());
        });
    
    public AppHost() : base(nameof(ProtocApi), typeof(ProtocServices).Assembly) { }

    // Configure your AppHost with the necessary configuration and dependencies your App needs
    public override void Configure(Container container)
    {
        SetConfig(new HostConfig
        {
            AllowFileExtensions = { "proto" },
        });
            
        var protocPath = Path.Combine(ContentRootDirectory.RealPath, "protoc");
        var exeDirPath = Env.IsWindows
            ? Path.Combine(protocPath, "win64")
            : Path.Combine(protocPath, "linux64");
        var protocConfig = new ProtocConfig {
            ExePath = Env.IsWindows
                ? Path.Combine(exeDirPath, "protoc.exe")
                : Path.Combine(exeDirPath, "protoc"),
            PluginPath = exeDirPath,
            ProtoIncludeDirectory = Path.Combine(protocPath, "include"),
            TempDirectory = Path.Combine(ContentRootDirectory.RealPath, "tmp"),
        }.Init();
        container.Register(protocConfig);

        if (Env.IsWindows)
            protocConfig.Languages.Remove(ProtocLang.Swift); // protoc-gen-grpc-swift does not exist on Windows

        Plugins.Add(new SharpPagesFeature {
            ScriptMethods = { new ProtocScriptMethods(protocConfig) }
        });
    }
}

public class ProtocScriptMethods : ScriptMethods
{
    private readonly ProtocConfig config;
    public ProtocScriptMethods(ProtocConfig config) => this.config = config;

    public List<KeyValuePair<string, string>> langs(ScriptScopeContext scope) =>
        (Context.GetServiceStackFilters().execService(scope, nameof(GetLanguages)) as GetLanguagesResponse)?.Results;
}
