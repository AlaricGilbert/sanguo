using Sanguo.Core.Communication;
using System;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace Sanguo.HubServer
{
    class Program
    {
        static void Main(string[] args)
        {
            SocketServer server = new SocketServer(2003);
            server.DataReceived += (s) =>
            {
                Console.WriteLine(Encoding.Default.GetString(s));
            };
            server.StartListen();
            for (int i = 0; i < 999; i++)
            {
                Socket client = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                try
                {
                    client.Connect(new IPEndPoint(IPAddress.Parse("127.0.0.1"), 2003));
                    client.Send(Encoding.Default.GetBytes("hello?"));
                }
                catch (Exception)
                {
                    byte[] buffer = new byte[2048];
                    client.Receive(buffer);
                    Console.WriteLine(Encoding.Default.GetString(buffer));
                }
            }
            while (true) { }
        }
    }
}
