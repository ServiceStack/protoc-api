using System;
using System.Collections.Generic;
using ProtocApi.ServiceModel;
using ServiceStack;
using ServiceStack.Text;

namespace ProtocApi.ServiceInterface
{
    public class ProtocConfig
    {
        public string ExePath { get; set; }
        public string PluginPath { get; set; }
        public string ProtoIncludeDirectory { get; set; }
        public string TempDirectory { get; set; }

        private string pluginPath(string name) => 
            '"' + (PluginPath.CombineWith(name) + (Env.IsWindows ? ".exe" : "")).Replace("\\","/") + '"';

        public Dictionary<Lang, ProtocOptions> Languages { get; set; }

        public ProtocConfig Init()
        {
            Languages = new Dictionary<Lang, ProtocOptions> {
                {
                    Lang.Cpp, new ProtocOptions("cpp", "C++") {
                        GrpcOutModifiers = new string[] { },
                        Args = new[] {
                            $"--plugin=protoc-gen-grpc={pluginPath("grpc_cpp_plugin")}"
                        }
                    }
                },
                {
                    Lang.CSharp, new ProtocOptions("csharp", "C#") {
                        GrpcOutModifiers = new string[] { },
                        Args = new[] {
                            $"--plugin=protoc-gen-grpc={pluginPath("grpc_csharp_plugin")}"
                        }
                    }
                },
                // requires installing https://pub.dev/packages/protoc_plugin on same server
                {
                    Lang.Dart, new ProtocOptions("dart", "Dart") {
                        OutModifiers = new []{ "grpc" }
                    }
                },
                // https://grpc.io/docs/reference/java/generated-code/
                {
                    Lang.Java, new ProtocOptions("java", "Java") {
                        Args = new[] {
                            $"--plugin={pluginPath("protoc-gen-grpc-java")}",
                            "--grpc-java_out=out",
                        }
                    }
                },
                // Java Lite (Android) https://github.com/protocolbuffers/protobuf/blob/master/java/lite.md
                {
                    Lang.JavaLite, new ProtocOptions("java", "Java (Lite)") {
                        OutModifiers = new[] { OutModifier.Lite },
                        Args = new[] {
                            $"--plugin={pluginPath("protoc-gen-grpc-java")}",
                            "--grpc-java_out=lite:out",
                        }
                    }
                },
                {
                    Lang.ObjectiveC, new ProtocOptions("objc", "Objective C") {
                        GrpcOutModifiers = new string[] { },
                        Args = new[] {
                            $"--plugin=protoc-gen-grpc={pluginPath("grpc_objective_c_plugin")}"
                        }
                    }
                },
                {
                    Lang.Php, new ProtocOptions("php", "PHP") {
                        GrpcOutModifiers = new string[] { },
                        Args = new[] {
                            $"--plugin=protoc-gen-grpc={pluginPath("grpc_php_plugin")}"
                        }
                    }
                },
                {
                    Lang.Python, new ProtocOptions("python", "Python") {
                        GrpcOutModifiers = new string[] { },
                        Args = new[] {
                            $"--plugin=protoc-gen-grpc={pluginPath("grpc_python_plugin")}"
                        }
                    }
                },
                {
                    Lang.Ruby, new ProtocOptions("ruby", "Ruby") {
                        GrpcOutModifiers = new string[] { },
                        Args = new[] {
                            $"--plugin=protoc-gen-grpc={pluginPath("grpc_python_plugin")}"
                        }
                    }
                }, 
                {
                    Lang.JavaScriptClosure, new ProtocOptions("js", "JavaScript (Closure)") {
                        OutModifiers = new[] {OutModifier.ImportStyleClosure},
                        GrpcWebModifiers = new[] {OutModifier.ImportStyleCommonJs, OutModifier.ModeGrpcWebText},
                    }
                }, 
                {
                    Lang.JavaScriptCommonJs, new ProtocOptions("js", "JavaScript (CommonJS)") {
                        OutModifiers = new[] {OutModifier.ImportStyleCommonJs},
                        GrpcWebModifiers = new[] {OutModifier.ImportStyleCommonJs, OutModifier.ModeGrpcWebText},
                    }
                },
                {
                    Lang.JavaScriptNodeJs, new ProtocOptions("js", "JavaScript (node.js)") {
                        OutModifiers = new[] {OutModifier.ImportStyleCommonJs,OutModifier.Binary},
                        GrpcOutModifiers = new string[] {},
                        Args = new[] {
                            $"--plugin=protoc-gen-grpc={pluginPath("grpc_node_plugin")}",
                        }
                    }
                },
                // in contrast to docs commonjs + .d.ts doesn't work
//                {
//                    Lang.JavaScriptCommonJsDts, new ProtocOptions("js", "JavaScript (CommonJS + .d.ts)") {
//                        OutModifiers = new[] {OutModifier.ImportStyleCommonJsDts},
//                        WebModifiers = new[] {OutModifier.ImportStyleCommonJs, OutModifier.ModeGrpcWebText},
//                    }
//                },
                {
                    Lang.TypeScript, new ProtocOptions("js", "TypeScript") {
                        OutModifiers = new[] {OutModifier.ImportStyleCommonJs},
                        GrpcWebModifiers = new[] {OutModifier.ImportStyleTypeScript, OutModifier.ModeGrpcWebText},
                    }
                }, 
                {
                    Lang.TypeScriptBinary, new ProtocOptions("js", "TypeScript (Binary)") {
                        OutModifiers = new[] {OutModifier.ImportStyleCommonJs, OutModifier.Binary},
                        GrpcWebModifiers = new[] {OutModifier.ImportStyleTypeScript, OutModifier.ModeGrpcWeb},
                    }
                }, 
                {
                    Lang.Go, new ProtocOptions("go", "Go") {
                        OutModifiers = new[] {OutModifier.PluginGo},
                        IndividuallyPerFile = true,
                    }
                },
                // only available in Linux https://github.com/grpc/grpc-swift/blob/nio/docs/quick-start.md
                // need to build with: swift build -Xcc -DTSI_OPENSSL_ALPN_SUPPORT=0 https://github.com/grpc/grpc-swift/pull/238
                // https://github.com/apple/swift-protobuf/blob/master/Documentation/PLUGIN.md
                {
                    Lang.Swift, new ProtocOptions("swift", "Swift") {
                        GrpcOutModifiers = new string[] {},
                        OutGrpcSubDir = "grpc",
                        Args = new[] {
                            $"--plugin=protoc-gen-grpc={pluginPath("protoc-gen-swift")}",
                            "--swift_opt=Visibility=Public",
                        }
                    }
                }
            };
            return this;
        }
    }
}
