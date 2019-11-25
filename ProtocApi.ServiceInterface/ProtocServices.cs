using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using ServiceStack;
using ProtocApi.ServiceModel;
using ServiceStack.DataAnnotations;
using ServiceStack.IO;
using ServiceStack.Logging;
using ServiceStack.Model;
using ServiceStack.Script;
using ServiceStack.Text;

namespace ProtocApi.ServiceInterface
{
    public class ProtocOptions
    {
        public string Lang { get; set; }
        public string Name { get; set; }
        public string[] OutModifiers { get; set; }
        public string[] WebModifiers { get; set; }
        
        public bool IndividuallyPerFile { get; set; }

        public ProtocOptions(string lang, string name)
        {
            Lang = lang;
            Name = name;
        }
    }
        
    public class ProtocServices : Service
    {
        public static ILog log = LogManager.GetLogger(typeof(ProtocServices));
        
        public object Any(Hello request) => new HelloResponse { Result = $"Hello, {request.Name}!" };

        public static Dictionary<Lang, ProtocOptions> LangConfig = new Dictionary<Lang, ProtocOptions> {
            { Lang.Cpp, new ProtocOptions("cpp","C++") },
            { Lang.CSharp, new ProtocOptions("csharp","C#") },
            { Lang.Java, new ProtocOptions("java","Java") },
            // Java Lite (Android) https://github.com/protocolbuffers/protobuf/blob/master/java/lite.md
            { Lang.JavaLite, new ProtocOptions("java","Java") { OutModifiers = new[] { OutModifier.Lite } } }, 
            { Lang.ObjectiveC, new ProtocOptions("objc","Objective C") }, 
            { Lang.Php, new ProtocOptions("php","PHP") },
            { Lang.Python, new ProtocOptions("python","Python") }, 
            { Lang.Ruby, new ProtocOptions("ruby","Ruby") },
            { Lang.JavaScriptClosure, new ProtocOptions("js","JavaScript (Closure)") {
                OutModifiers = new[] { OutModifier.ImportStyleClosure },
            } },
            { Lang.JavaScriptCommonJs, new ProtocOptions("js","JavaScript (CommonJS)") {
                OutModifiers = new[] { OutModifier.ImportStyleCommonJs },
                WebModifiers = new [] { OutModifier.ImportStyleCommonJs, OutModifier.ModeGrpcWebText },
            } },
//            { Lang.JavaScriptCommonJsDts, new ProtocOptions("js","JavaScript (CommonJS + .d.ts)") {
//                OutModifiers = new [] { OutModifier.ImportStyleCommonJsDts },
//                WebModifiers = new [] { OutModifier.ImportStyleCommonJs, OutModifier.ModeGrpcWebText },
//            } },
            { Lang.TypeScript, new ProtocOptions("js","TypeScript") {
                OutModifiers = new [] { OutModifier.ImportStyleCommonJs }, 
                WebModifiers = new [] { OutModifier.ImportStyleTypeScript, OutModifier.ModeGrpcWebText },
            } },
            { Lang.TypeScriptBinary, new ProtocOptions("js","TypeScript (Binary)") {
                OutModifiers = new [] { OutModifier.ImportStyleCommonJs, OutModifier.Binary }, 
                WebModifiers = new [] { OutModifier.ImportStyleTypeScript, OutModifier.ModeGrpcWeb },
            } },
            { Lang.Go, new ProtocOptions("go","Go") {
                OutModifiers = new[]{ OutModifier.PluginGo },
                IndividuallyPerFile = true,
            }  },
        }; 
        
        public ProtocConfig ProtocConfig { get; set; }
        
        public object Any(Protoc request)
        {
            var files = request.Files; 
            if (request.Files.IsEmpty())
            {
                if (Request.Files.IsEmpty()) // allow Files uploaded by HTTP POST
                    throw new ArgumentNullException(nameof(request.Files));
                
                files = new Dictionary<string, string>();
                foreach (var httpFile in Request.Files)
                {
                    files[httpFile.FileName ?? httpFile.Name] = httpFile.InputStream.ReadToEnd();
                }
            }

            var tmpId = (Request as IHasStringId)?.Id ?? Guid.NewGuid().ToString();
            var tmpPath = Path.Combine(ProtocConfig.TempDirectory, tmpId);
            var outPath = Path.Combine(tmpPath, "out");
            try { Directory.CreateDirectory(outPath); } catch {}
            
            var fs = new FileSystemVirtualFiles(tmpPath);
            fs.WriteFiles(files);

            var langOptions = LangConfig[request.Lang];
            var args = StringBuilderCache.Allocate();

            var outArgs = "";
            if (!langOptions.OutModifiers.IsEmpty())
            {
                foreach (var modifier in langOptions.OutModifiers)
                {
                    if (outArgs.Length > 0)
                        outArgs += ",";
                    outArgs += modifier;
                }
                if (outArgs.Length > 0)
                    outArgs += ":";
            }
            
            args.AppendFormat($"-I . -I \"{ProtocConfig.ProtoIncludeDirectory}\" --{langOptions.Lang}_out={outArgs}out");

            if (!langOptions.WebModifiers.IsEmpty())
            {
                var webArgs = "";
                foreach (var modifier in langOptions.WebModifiers)
                {
                    if (webArgs.Length > 0)
                        webArgs += ",";
                    webArgs += modifier;
                }
                if (webArgs.Length > 0)
                    webArgs += ":";
                args.AppendFormat($" --grpc-web_out={webArgs}out");
            }

            if (!langOptions.IndividuallyPerFile)
            {
                foreach (var entry in files)
                {
                    args.Append($" {entry.Key}");
                }
            
                exec(tmpPath, StringBuilderCache.ReturnAndFree(args));
            }
            else
            {
                var argsBase = StringBuilderCache.ReturnAndFree(args);
                foreach (var entry in files)
                {
                    exec(tmpPath, $"{argsBase} {entry.Key}");
                }
            }

            var response = new ProtocResponse {
                GeneratedFiles = new Dictionary<string, string>()
            };

            var fsOut = new FileSystemVirtualFiles(Path.Combine(tmpPath, "out"));
            var genFiles = fsOut.GetAllFiles();
            foreach (var virtualFile in genFiles)
            {
                response.Lang = request.Lang;
                response.GeneratedFiles[virtualFile.VirtualPath] = virtualFile.GetTextContentsAsMemory().ToString();
            }

            return response;
        }

        private void exec(string tmpPath, string args)
        {
            var process = new Process {
                StartInfo = {
                    FileName = Path.Combine(ProtocConfig.WorkingDirectory, ProtocConfig.ExeName),
                    UseShellExecute = false,
                    CreateNoWindow = true,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    WorkingDirectory = tmpPath,
                    Arguments = args,
                }
            };

            if (log.IsDebugEnabled)
                log.Debug($"{ProtocConfig.ExeName} {process.StartInfo.Arguments}");

            using (process)
            {
                process.Start();

                var output = process.StandardOutput.ReadToEnd();
                var error = process.StandardError.ReadToEnd();

                process.WaitForExit();
                process.Close();

                if (!string.IsNullOrEmpty(error))
                    throw new Exception($"`protoc {process.StartInfo.Arguments}` command failed: {error}\n\n{output}");
            }
        }
    }
}
