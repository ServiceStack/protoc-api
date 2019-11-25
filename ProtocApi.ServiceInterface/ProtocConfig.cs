using System.Collections.Generic;

namespace ProtocApi.ServiceInterface
{
    public class ProtocConfig
    {
        public string ExeName { get; set; }
        public string WorkingDirectory { get; set; }
        public string ProtoIncludeDirectory { get; set; }
        public string TempDirectory { get; set; }
    }
}
