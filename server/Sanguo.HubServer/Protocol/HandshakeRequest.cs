using System.Net.Sockets;
using Sanguo.Core.Communication;
using System.Text;
using Newtonsoft.Json;

namespace Sanguo.HubServer.Protocol
{
    public class HandshakeRequest : Request
    {
        public override void Handle(Socket senderSocket)
        {
            string jsonResponse = JsonConvert.SerializeObject(HandshakeResponse.Default);
            senderSocket.Send(Encoding.Default.GetBytes(jsonResponse));
        }
        public static string MagicMessage { get; } = "PI=3.1415926,e=2.71828";
        public static HandshakeRequest Default { get; } = new HandshakeRequest { RequestMessage = MagicMessage };
    }
}
