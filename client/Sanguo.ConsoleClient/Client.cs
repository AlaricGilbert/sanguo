using Newtonsoft.Json;
using Sanguo.Core.Communication;
using Sanguo.Core.Protocol;
using System;
using System.Net;

namespace Sanguo.ConsoleClient
{
    public static class Client
    {
        public static void Run()
        {

            bool waiting = true;
            IOCPClient client = new IOCPClient(IPAddress.Parse("127.0.0.1"), 18112);
            client.DataReceived += (sender, e) =>
            {
                string info = e.GetReceived();
                Console.WriteLine(info);
                waiting = false;
            };
            client.Connect();
            client.Listen();


            LoginRequest loginRequest = new LoginRequest
            {
                Username = "alaric",
                HashedPw = "666",
                RequestType = typeof(LoginRequest).ToString()
            };

            string s = JsonConvert.SerializeObject(loginRequest);
            Console.WriteLine(s);
            client.Send(s);

            while (waiting) { }

            LoginRequest loginRequest1 = new LoginRequest
            {
                Username = "alaric",
                HashedPw = "777",
                RequestType = typeof(LoginRequest).ToString()
            };

            string sp = JsonConvert.SerializeObject(loginRequest1);
            Console.WriteLine(sp);
            client.Send(sp);

        }
    }
}
