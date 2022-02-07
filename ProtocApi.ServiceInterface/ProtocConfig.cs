using System;
using System.Collections.Generic;
using ProtocApi.ServiceModel;
using ServiceStack;
using ServiceStack.Text;

namespace ProtocApi.ServiceInterface
{
    public static class OutModifier
    {
        public static string Lite = "lite";
        public static string Binary = "binary";
        public static string PluginGo = "plugins=grpc";
        public static string ImportStyleClosure = "import_style=closure";
        public static string ImportStyleCommonJs = "import_style=commonjs";
        public static string ImportStyleCommonJsDts = "import_style=commonjs+dts";
        public static string ImportStyleTypeScript = "import_style=typescript";
        public static string ModeGrpcWeb = "mode=grpcweb";
        public static string ModeGrpcWebText = "mode=grpcwebtext";
    }

    public class ProtocConfig
    {
        public string ExePath { get; set; }
        public string PluginPath { get; set; }
        public string ProtoIncludeDirectory { get; set; }
        public string TempDirectory { get; set; }

        private string pluginPath(string name) => 
            '"' + (PluginPath.CombineWith(name) + (Env.IsWindows ? ".exe" : "")).Replace("\\","/") + '"';

        public Dictionary<ProtocLang, ProtocOptions> Languages { get; set; }

        public ProtocConfig Init()
        {
            Languages = new Dictionary<ProtocLang, ProtocOptions> {
                {
                    ProtocLang.Cpp, new ProtocOptions("cpp", "C++") {
                        GrpcOutModifiers = new string[] { },
                        Args = new[] {
                            $"--plugin=protoc-gen-grpc={pluginPath("grpc_cpp_plugin")}"
                        }
                    }
                },
                {
                    ProtocLang.CSharp, new ProtocOptions("csharp", "C#") {
                        GrpcOutModifiers = new string[] { },
                        Args = new[] {
                            $"--plugin=protoc-gen-grpc={pluginPath("grpc_csharp_plugin")}"
                        }
                    }
                },
                // requires installing https://pub.dev/packages/protoc_plugin on same server
                {
                    ProtocLang.Dart, new ProtocOptions("dart", "Dart") {
                        OutModifiers = new []{ "grpc" }
                    }
                },
                // https://grpc.io/docs/reference/java/generated-code/
                {
                    ProtocLang.Java, new ProtocOptions("java", "Java") {
                        Args = new[] {
                            $"--plugin=protoc-gen-grpc-java={pluginPath("protoc-gen-grpc-java")}",
                            "--grpc-java_out=out",
                        }
                    }
                },
                // Java Lite (Android) https://github.com/protocolbuffers/protobuf/blob/master/java/lite.md
                {
                    ProtocLang.JavaLite, new ProtocOptions("java", "Java (Lite)") {
                        OutModifiers = new[] { OutModifier.Lite },
                        Args = new[] {
                            $"--plugin=protoc-gen-grpc-java={pluginPath("protoc-gen-grpc-java")}",
                            "--grpc-java_out=lite:out",
                        }
                    }
                },
                {
                    ProtocLang.ObjectiveC, new ProtocOptions("objc", "Objective C") {
                        GrpcOutModifiers = new string[] { },
                        Args = new[] {
                            $"--plugin=protoc-gen-grpc={pluginPath("grpc_objective_c_plugin")}"
                        }
                    }
                },
                {
                    ProtocLang.Php, new ProtocOptions("php", "PHP") {
                        GrpcOutModifiers = new string[] { },
                        Args = new[] {
                            $"--plugin=protoc-gen-grpc={pluginPath("grpc_php_plugin")}"
                        }
                    }
                },
                {
                    ProtocLang.Python, new ProtocOptions("python", "Python") {
                        GrpcOutModifiers = new string[] { },
                        Args = new[] {
                            $"--plugin=protoc-gen-grpc={pluginPath("grpc_python_plugin")}"
                        }
                    }
                },
                {
                    ProtocLang.Ruby, new ProtocOptions("ruby", "Ruby") {
                        GrpcOutModifiers = new string[] { },
                        Args = new[] {
                            $"--plugin=protoc-gen-grpc={pluginPath("grpc_ruby_plugin")}"
                        }
                    }
                }, 
                {
                    ProtocLang.JavaScriptClosure, new ProtocOptions("js", "JavaScript (Closure)") {
                        OutModifiers = new[] {OutModifier.ImportStyleClosure},
                        GrpcWebModifiers = new[] {OutModifier.ImportStyleCommonJs, OutModifier.ModeGrpcWebText},
                    }
                }, 
                {
                    ProtocLang.JavaScriptCommonJs, new ProtocOptions("js", "JavaScript (CommonJS)") {
                        OutModifiers = new[] {OutModifier.ImportStyleCommonJs},
                        GrpcWebModifiers = new[] {OutModifier.ImportStyleCommonJs, OutModifier.ModeGrpcWebText},
                    }
                },
                {
                    ProtocLang.JavaScriptNodeJs, new ProtocOptions("js", "JavaScript (node.js)") {
                        OutModifiers = new[] {OutModifier.ImportStyleCommonJs,OutModifier.Binary},
                        GrpcOutModifiers = new string[] {},
                        Args = new[] {
                            $"--plugin=protoc-gen-grpc={pluginPath("grpc_node_plugin")}",
                        }
                    }
                },
                // in contrast to docs commonjs + .d.ts doesn't work
//                {
//                    ProtocLang.JavaScriptCommonJsDts, new ProtocOptions("js", "JavaScript (CommonJS + .d.ts)") {
//                        OutModifiers = new[] {OutModifier.ImportStyleCommonJsDts},
//                        WebModifiers = new[] {OutModifier.ImportStyleCommonJs, OutModifier.ModeGrpcWebText},
//                    }
//                },
                {
                    ProtocLang.TypeScript, new ProtocOptions("js", "TypeScript") {
                        OutModifiers = new[] {OutModifier.ImportStyleCommonJs},
                        GrpcWebModifiers = new[] {OutModifier.ImportStyleTypeScript, OutModifier.ModeGrpcWebText},
                    }
                }, 
                {
                    ProtocLang.TypeScriptBinary, new ProtocOptions("js", "TypeScript (Binary)") {
                        OutModifiers = new[] {OutModifier.ImportStyleCommonJs, OutModifier.Binary},
                        GrpcWebModifiers = new[] {OutModifier.ImportStyleTypeScript, OutModifier.ModeGrpcWeb},
                    }
                }, 
                {
                    ProtocLang.Go, new ProtocOptions("go", "Go") {
                        OutModifiers = new[] {OutModifier.PluginGo},
                        IndividuallyPerFile = true,
                    }
                },
                // only available in Linux https://github.com/grpc/grpc-swift/blob/nio/docs/quick-start.md
                // need to build with: swift build -Xcc -DTSI_OPENSSL_ALPN_SUPPORT=0 https://github.com/grpc/grpc-swift/pull/238
                // https://github.com/apple/swift-protobuf/blob/master/Documentation/PLUGIN.md
                // https://github.com/grpc/grpc-swift
                {
                    ProtocLang.Swift, new ProtocOptions("swift", "Swift") {
                        Args = new[] {
                            $"--plugin={pluginPath("protoc-gen-swift")}",
                            "--swift_opt=Visibility=Public",
                            "--grpc-swift_opt=Visibility=Public",
                            "--grpc-swift_out=Client=true,Server=false:out",
                        }
                    }
                }
            };
            return this;
        }
    }
}
