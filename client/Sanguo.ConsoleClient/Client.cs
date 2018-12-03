using Newtonsoft.Json;
using Sanguo.Core.Communication;
using Sanguo.Core.Protocol;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace Sanguo.ConsoleClient
{
    public static class Client
    {
        static IOCPClient client = new IOCPClient(IPAddress.Parse("127.0.0.1"), 18112);
        static Dictionary<string, Action<string>> ResponseHandler = new Dictionary<string, Action<string>>(); 
        static bool waiting = true;
        static int hsErrorCount = 0;
        public static void Run()
        {
            addHandlers();
            client.DataReceived += (sender, e) =>
            {
                string response = e.GetReceived();
                Response r = JsonConvert.DeserializeObject<Response>(response);
                ResponseHandler[r.ResponseType](response);

                waiting = false;
            };
            client.Connect();
            client.Listen();
            run();
        }

        private static void addHandlers()
        {
            void handshakeHandler(string jsonResp)
            {
                HandshakeResponse r = JsonConvert.DeserializeObject<HandshakeResponse>(jsonResp);
                if (r.Status)
                {
                    Program.logger.Write("Handshake finished", "Client/CommuniHandler", Core.Logger.LogLevel.Infos);
                    return;
                }
                Program.logger.Write("Handshake error:\n" + jsonResp + "\nRetry in 1 second", "Client/CommuniHandler", Core.Logger.LogLevel.Warns);
                hsErrorCount++;
                if (hsErrorCount >= 5)
                {
                    Program.logger.Write("After 5 times of trying, handshake still failed, program will exit after 5 seconds.", "Client/CommuniHandler", Core.Logger.LogLevel.Error);
                    Thread.Sleep(5000);
                    Environment.Exit(-1);
                }

                Thread.Sleep(1000);
                client.Send(JsonConvert.SerializeObject(HandshakeRequest.Default));
            }
            ResponseHandler.Add(typeof(HandshakeResponse).ToString(), handshakeHandler);
            void loginHandler(string jsonResp)
            {
                Program.logger.Write("log-ok", "Client/CommuniHandler", Core.Logger.LogLevel.Infos);
            }
            ResponseHandler.Add(typeof(LoginResponse).ToString(), loginHandler);
            void availableLobbyHandler(string jsonResp)
            {
                Program.logger.Write("lobby-got ok.", "Client/CommuniHandler", Core.Logger.LogLevel.Infos);
                AvailableLobbiesResponse r = JsonConvert.DeserializeObject<AvailableLobbiesResponse>(jsonResp);
                string ip = "";
                int port = 0;
                foreach (var item in r.LobbyInfos)
                {
                    ip = item.Value.IP;
                    port = item.Value.Port;
                    break;
                }
                IOCPClient client = new IOCPClient(IPAddress.Parse(ip), port);
                client.Connect();
                client.Listen();
            }
            ResponseHandler.Add(typeof(AvailableLobbiesResponse).ToString(), availableLobbyHandler);
        }

        static void run()
        {
            //Hand shaking.
            client.Send(JsonConvert.SerializeObject(HandshakeRequest.Default));
            while (waiting) { }

            //Login
            LoginRequest loginRequest = new LoginRequest
            {
                Username = "alaric",
                HashedPw = "666",
                RequestType = typeof(LoginRequest).ToString()
            };
            client.Send(JsonConvert.SerializeObject(loginRequest));
            waiting = true;
            while (waiting) { }

            //Get available servers.
#if DEBUG
            //Waiting for test lobby loading.
            Thread.Sleep(1000);
#endif
            AvailableLobbiesRequest r = AvailableLobbiesRequest.Default;
            client.Send(JsonConvert.SerializeObject(r));
        }
    }
}
