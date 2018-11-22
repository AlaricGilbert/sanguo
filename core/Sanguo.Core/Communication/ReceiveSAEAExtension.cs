using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Text;

namespace Sanguo.Core.Communication
{
    public static class ReceiveSAEAExtension
    {
        public static Socket GetSendSocket(this SocketAsyncEventArgs args)
        {
            return ((ReceiveSAEAUToken)args.UserToken).
        }
    }
}
