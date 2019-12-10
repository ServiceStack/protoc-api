using System.Collections.Generic;
using System.Runtime.Serialization;
using ServiceStack;

namespace ProtocApi.ServiceModel
{
    [Route("/protoc/{Lang}")]
    public class Protoc : IReturn<ProtocResponse>
    {
        public Lang Lang { get; set; }
        public Dictionary<string, string> Files { get; set; }
        public string ProtoUrl { get; set; }
    }
    
    public class ProtocResponse
    {
        public Lang Lang { get; set; }
        public Dictionary<string, string> GeneratedFiles { get; set; }
        public string ArchiveUrl { get; set; }
        public ResponseStatus ResponseStatus { get; set; }
    }

    [DataContract]
    public enum ImportStyle
    {
        [EnumMember(Value = "closure")]
        Closure,
        [EnumMember(Value = "commonjs")]
        CommonJs,
        [EnumMember(Value = "commonjs+dts")]
        CommonJsDts,
        [EnumMember(Value = "binary")]
        Binary,
        [EnumMember(Value = "typescript")]
        TypeScript,
    }

    [DataContract]
    public enum WebMode
    {
        [EnumMember(Value = "grpcweb")]
        GrpcWeb,
        [EnumMember(Value = "grpcwebtext")]
        GrpcWebText,
    }

    [DataContract]
    public enum Lang
    {
        [EnumMember(Value = "cpp")]
        Cpp,
        [EnumMember(Value = "csharp")]
        CSharp,
        [EnumMember(Value = "dart")]
        Dart,
        [EnumMember(Value = "java")]
        Java,
        [EnumMember(Value = "java-lite")]
        JavaLite,
        [EnumMember(Value = "objc")]
        ObjectiveC,
        [EnumMember(Value = "php")]
        Php,
        [EnumMember(Value = "python")]
        Python,
        [EnumMember(Value = "ruby")]
        Ruby,
        [EnumMember(Value = "go")]
        Go,
        [EnumMember(Value = "js-closure")]
        JavaScriptClosure,
        [EnumMember(Value = "js-commonjs")]
        JavaScriptCommonJs,
        [EnumMember(Value = "js-node")]
        JavaScriptNodeJs,
//        [EnumMember(Value = "js-tsd")] // doesn't work, uses closure
//        JavaScriptCommonJsDts,
        [EnumMember(Value = "swift")]
        Swift,
        [EnumMember(Value = "ts")]
        TypeScript,
        [EnumMember(Value = "ts-binary")]
        TypeScriptBinary,
    }
}