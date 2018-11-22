using System.Net.Sockets;

namespace Sanguo.Core.Communication
{
    public class Request
    {
        public string RequestMessage { get; set; }
        public virtual void Handle(Socket senderSocket) { }
    }
}
