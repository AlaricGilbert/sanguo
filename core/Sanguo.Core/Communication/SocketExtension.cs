using System;
using System.Collections.Generic;
using System.Text;
using System.Net.Sockets;
using Newtonsoft.Json;

namespace Sanguo.Core.Communication
{
    static class SocketExtension
    {
        public static void SendResponse(this SocketAsyncEventArgs arg, Response r) =>
            ((ReceiveSAEAUToken)arg.UserToken)
                .Socket.Send(Encoding.Default.GetBytes(JsonConvert.SerializeObject(r)));
    }
}
