using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
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
        
        public string[] Args { get; set; }

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

        public ProtocConfig ProtocConfig { get; set; }
        
        public object Any(Protoc request)
        {
            var tmpId = (Request as IHasStringId)?.Id ?? Guid.NewGuid().ToString();
            var files = request.Files; 
            if (request.Files.IsEmpty())
            {
                if (Request.Files.IsEmpty()) // allow Files uploaded by HTTP POST
                    throw new ArgumentNullException(nameof(request.Files));
                
                files = new Dictionary<string, string>();
                foreach (var httpFile in Request.Files)
                {
                    var fileName = httpFile.FileName ?? httpFile.Name;
                    if (!fileName.EndsWith(".proto") && !fileName.EndsWith(".zip"))
                        throw new ArgumentException($"Unsupported file '{fileName}'. Only .proto or .zip files supported.");

                    if (fileName.EndsWith(".zip"))
                    {
                        var tmpZipPath = Path.GetTempFileName();
                        var tmpDir = Path.Combine(Path.GetTempPath(), "protoc-api", tmpId);
                        httpFile.SaveTo(tmpZipPath);
                        
                        ZipFile.ExtractToDirectory(tmpZipPath, tmpDir);
                        
                        var tmpDirInfo = new DirectoryInfo(tmpDir);
                        var hasProtoRootDir = tmpDirInfo.GetFiles("*.proto").Length > 0;
                        var fsZipDir = !hasProtoRootDir && tmpDirInfo.GetDirectories().Length == 1
                            ? new FileSystemVirtualFiles(tmpDirInfo.GetDirectories()[0].FullName)
                            : new FileSystemVirtualFiles(tmpDirInfo.FullName);

                        foreach (var file in fsZipDir.GetAllFiles().Where(x => x.Extension == "proto"))
                        {
                            files[file.VirtualPath] = file.ReadAllText();
                        }
                    }
                    else
                    {
                        files[fileName] = httpFile.InputStream.ReadToEnd();
                    }
                }
            }

            var tmpPath = Path.Combine(ProtocConfig.TempDirectory, tmpId);
            var outPath = Path.Combine(tmpPath, "out");
            try { Directory.CreateDirectory(outPath); } catch {}
            
            var fs = new FileSystemVirtualFiles(tmpPath);
            fs.WriteFiles(files);

            var langOptions = ProtocConfig.Languages[request.Lang];
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

            if (!langOptions.Args.IsEmpty())
            {
                foreach (var arg in langOptions.Args)
                {
                    args.Append($" {arg}");
                }
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

            var serviceName = files.Count == 1 ? files.Keys.First() : "grpc";
            var response = new ProtocResponse {
                GeneratedFiles = new Dictionary<string, string>(),
                ArchiveUrl = Request.ResolveAbsoluteUrl(new GetArchive { RequestId = tmpId, FileName = $"{serviceName}.{langOptions.Lang}.zip" }.ToGetUrl()),
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

        public object Any(GetArchive request)
        {
            if (string.IsNullOrEmpty(request.RequestId))
                throw new ArgumentNullException(nameof(request.RequestId));
            
            var tmpPath = Path.Combine(ProtocConfig.TempDirectory, request.RequestId, "out");
            if (!Directory.Exists(tmpPath))
                throw HttpError.NotFound("Temporary archive no longer exists");

            var tmpZipPath = Path.Combine(ProtocConfig.TempDirectory, request.RequestId, request.FileName ?? "grpc.zip");
            if (!File.Exists(tmpZipPath))
            {
                ZipFile.CreateFromDirectory(tmpPath, tmpZipPath);
            }
            
            return new HttpResult(new FileInfo(tmpZipPath), asAttachment:true);
        }
    }
}
