using ServiceStack;

namespace ProtocApi.ServiceModel
{
    [Route("/archive/{RequestId}/{FileName}")]
    public class GetArchive : IReturn<byte[]>
    {
        public string RequestId { get; set; }
        public string FileName { get; set; }
    }
}