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
            SocketServer server = new SocketServer(2003);
            
            server.DataReceived += (sender, s) =>
            {
                string str = Encoding.Default.GetString(s);
                Request r = JsonConvert.DeserializeObject<Request>(str);
                Console.WriteLine(str);
                sender.ConnectSocket.Send(Encoding.Default.GetBytes("D"));
                sender.GetSendSocket().Send(Encoding.Default.GetBytes("D"));
                if (r.RequestMessage == HandshakeRequest.MagicMessage)
                    JsonConvert.DeserializeObject<HandshakeRequest>(str).Handle(sender.GetSendSocket());
            };
            server.StartListen();





            for (int i = 0; i < 10; i++)
            {
                Socket client = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                try
                {
                    client.Connect(new IPEndPoint(IPAddress.Parse("127.0.0.1"), 2003));
                    string json = JsonConvert.SerializeObject(HandshakeRequest.Default);

                    client.Send(Encoding.Default.GetBytes(json));
                    byte[] buffer = new byte[2048];
                    client.Receive(buffer);
                    Console.WriteLine(Encoding.Default.GetString(buffer));
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                }
            }
            while (true) { }
        }
    }
}
