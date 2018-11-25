using System;
using System.Text;
using System.Net.Sockets;

namespace Sanguo.Core.Communication
{
    public static class SocketAsyncEventArgsExtension
    {
        public static string GetReceived(this SocketAsyncEventArgs args)
        {
            byte[] data = new byte[args.BytesTransferred];
            Array.Copy(args.Buffer, args.Offset, data, 0, data.Length);
            string info = Encoding.Default.GetString(data);
            return info;
        }
    }
}
