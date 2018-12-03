using Newtonsoft.Json;
using Sanguo.Core.Communication;
using Sanguo.Core.Protocol;
using System;
using System.Net;
using System.Threading;

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
                Program.logger.Write(info, "ConsoleClient/DataReceived", Core.Logger.LogLevel.Infos);
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
            client.Send(s);
            waiting = true;
            while (waiting) { }
            Thread.Sleep(1000);
            AvailableLobbiesRequest r = AvailableLobbiesRequest.Default;
            client.Send(JsonConvert.SerializeObject(r));
        }
    }
}
