using Newtonsoft.Json;
using Sanguo.Core.Communication;
using System.Net.Sockets;

namespace Sanguo.Core.Protocol
{
    public class HandshakeRequest : Request
    {
        [JsonIgnore]
        public static string MagicMessage { get; } = "PI=3.1415926,e=2.71828";
        [JsonIgnore]
        public static readonly HandshakeRequest Default = new HandshakeRequest { RequestMessage = MagicMessage, RequestType = typeof(HandshakeRequest).ToString() };
    }
}
