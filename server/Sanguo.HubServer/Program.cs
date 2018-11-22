using Sanguo.Core.Communication;
using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using Newtonsoft.Json;
using Sanguo.HubServer.Protocol;

namespace Sanguo.HubServer
{
    class Program
    {
        static void Main(string[] args)
        {
            IOCPServer server = new IOCPServer(8088, 1000);
            server.DataReceived += (sender, e) =>
            {
                byte[] data = new byte[e.BytesTransferred];
                Array.Copy(e.Buffer, e.Offset, data, 0, data.Length);

                string info = Encoding.Default.GetString(data);
                //Log4Debug(String.Format("收到 {0} 数据为 {1}", s.RemoteEndPoint.ToString(), info));
                Console.WriteLine("server got:{0}",info);
                Socket s = e.UserToken as Socket;
                SocketAsyncEventArgs a = new SocketAsyncEventArgs();
                a.SetBuffer(data, 0, data.Length);
                s.SendAsync(a);
                
            };
            server.Init();
            server.Start();
            IPAddress local = IPAddress.Parse("127.0.0.1");

            IOCPClient client = new IOCPClient(local,8088);
            client.DataReceived += (sender, e) => {
                byte[] data = new byte[e.BytesTransferred];
                Array.Copy(e.Buffer, e.Offset, data, 0, data.Length);
                string info = Encoding.Default.GetString(data);
                Console.WriteLine("client got:{0}",info);
            };
            client.Connect();
            client.Listen();

            client.Send(Encoding.Default.GetBytes("dsa"));            

            while (true) { }
        }
    }
}
