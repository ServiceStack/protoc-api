using System.Collections.Generic;
using System.Runtime.Serialization;
using ServiceStack;

namespace ProtocApi.ServiceModel
{
    [Route("/langs", "GET")]
    [DataContract]
    public class GetLanguages { }

    [DataContract]
    public class GetLanguagesResponse
    {
        [DataMember]
        public List<KeyValuePair<string, string>> Results { get; set; }
    }
}