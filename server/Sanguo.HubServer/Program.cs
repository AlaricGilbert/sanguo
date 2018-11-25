using Sanguo.Core.Communication;
using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace Sanguo.HubServer
{
    class Program
    {
        static void Main(string[] args)
        {

            IOCPServer server = new IOCPServer(8088, 10000);
            server.DataReceived += (sender, e) =>
            {
                string info = e.GetReceived();
                //Console.WriteLine("server got:{0}",info);
                ((IOCPServer)sender).Send(e, Encoding.Default.GetBytes(info));
            };
            server.Init();
            server.Start();
            DateTime t = DateTime.Now;
            for (int i = 0; i < 10000; i++)
            {
                IOCPClient client = new IOCPClient(IPAddress.Parse("127.0.0.1"), 8088);
                client.DataReceived += (sender, e) => {
                    string info = e.GetReceived();
                    ((IOCPClient)sender).Close();
                    if (i == 9999)
                        Console.WriteLine((DateTime.Now-t).TotalMilliseconds);
                };
                client.Connect(i);
                client.Listen();
                client.Send(Encoding.Default.GetBytes(i.ToString()));
            }
            Console.ReadKey();
        }
    }
}
