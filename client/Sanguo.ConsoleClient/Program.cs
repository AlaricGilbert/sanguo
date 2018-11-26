using Newtonsoft.Json;
using Sanguo.Core.Communication;
using Sanguo.Core.Protocol;
using System;
using System.Net;

namespace Sanguo.ConsoleClient
{
    class Program
    {
        static void Main(string[] args)
        {
            IOCPClient client = new IOCPClient(IPAddress.Parse("127.0.0.1"), 18112);
            client.DataReceived += (sender, e) => {
                string info = e.GetReceived();
                Console.WriteLine(info);
                ((IOCPClient)sender).Close();
            };
            client.Connect();
            client.Listen();

            HandshakeRequest request = HandshakeRequest.Default;
            string s = JsonConvert.SerializeObject(request);
            Console.WriteLine(s);
            client.Send(s);
        }
    }
}
