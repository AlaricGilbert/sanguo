using Newtonsoft.Json;
using Sanguo.Core.Communication;
using Sanguo.Core.Protocol;
using System;
using System.Net;

namespace Sanguo.ConsoleClient
{
    public static class Client
    {
        static IOCPClient client = new IOCPClient(IPAddress.Parse("127.0.0.1"), 18112);
        static bool waiting = true;
        public static void Run()
        {
            client.DataReceived += (sender, e) =>
            {
                string info = e.GetReceived();
                Console.WriteLine(info);
                waiting = false;
            };
            client.Connect();
            client.Listen();
            HandShake();
            Login();
        }
        static void HandShake()
        {
            HandshakeRequest handshakeRequest = HandshakeRequest.Default;
            string s = JsonConvert.SerializeObject(handshakeRequest);
            Console.WriteLine(s);
            client.Send(s);
        }
        static void Login()
        {
            while (waiting) { }
            LoginRequest loginRequest = new LoginRequest
            {
                Username = "alaric",
                HashedPw = "666",
                RequestType = typeof(LoginRequest).ToString()
            };

            string s = JsonConvert.SerializeObject(loginRequest);
            Console.WriteLine(s);
            client.Send(s);
            waiting = true;


            while (waiting) { }

            LoginRequest loginRequest1 = new LoginRequest
            {
                Username = "alaric",
                HashedPw = "777",
                RequestType = typeof(LoginRequest).ToString()
            };

            s = JsonConvert.SerializeObject(loginRequest1);
            Console.WriteLine(s);
            client.Send(s);
        }
    }
}
