using Newtonsoft.Json;

namespace Sanguo.Core.Protocol.Common
{
    public class HandshakeRequest : Request
    {
        [JsonIgnore]
        public const string MagicMessage = "PI=3.1415926,e=2.71828";
        [JsonIgnore]
        public static readonly HandshakeRequest Default = new HandshakeRequest { RequestMessage = MagicMessage, RequestType = typeof(HandshakeRequest).ToString() };
    }
}
