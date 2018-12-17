using Newtonsoft.Json;
using Sanguo.Core.Communication;
using Sanguo.Core.Protocol.Common;
using Sanguo.Core.Protocol.Hub;
using Sanguo.Core.Protocol.Lobby;
using System;
using System.Collections.Generic;
using System.Net;
using System.Threading;

namespace Sanguo.ConsoleClient
{
    public static class Client
    {
        static IOCPClient client = new IOCPClient(IPAddress.Parse("127.0.0.1"), 18112);
        static Dictionary<string, Action<string>> HubResponseHandler = new Dictionary<string, Action<string>>();
        static Dictionary<string, Action<string>> LobbyResponseHandler = new Dictionary<string, Action<string>>();
        static bool waiting = true;
        static int hsErrorCount = 0;
        public static void Run()
        {
            addHandlers();
            client.DataReceived += (sender, e) =>
            {
                string response = e.GetReceived();
                Response r = JsonConvert.DeserializeObject<Response>(response);
                HubResponseHandler[r.ResponseType](response);

                waiting = false;
            };
            client.Connect();
            client.Listen();
            run();
        }

        private static void addHandlers()
        {
            #region Handshake
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
            HubResponseHandler.Add(typeof(HandshakeResponse).ToString(), handshakeHandler);
            #endregion

            #region Log-in
            void loginHandler(string jsonResp)
            {
                Program.logger.Write("log-ok", "Client/CommuniHandler", Core.Logger.LogLevel.Infos);
            }
            HubResponseHandler.Add(typeof(LoginResponse).ToString(), loginHandler);
            #endregion

            void availableRoomHandler(string s)
            {
                AvailableRoomsResponse resp = JsonConvert.DeserializeObject<AvailableRoomsResponse>(s);
                foreach (var item in resp.RoomInfos)
                {
                    Console.WriteLine(item.Identity);
                }
            }
            LobbyResponseHandler.Add(typeof(AvailableRoomsResponse).ToString(), availableRoomHandler);
            #region Available lobbies
            void availableLobbyHandler(string jsonResp)
            {
                Program.logger.Write("lobby-got ok.", "Client/CommuniHandler", Core.Logger.LogLevel.Infos);
                AvailableLobbiesResponse r = JsonConvert.DeserializeObject<AvailableLobbiesResponse>(jsonResp);
                #region Connect
                string ip = "";
                int port = 0;
                foreach (var item in r.LobbyInfos)
                {
                    ip = item.Value.IP;
                    port = item.Value.Port;
                    break;
                }
               
                IOCPClient _client = new IOCPClient(IPAddress.Parse(ip), port);
                _client.Connect();
                _client.Listen();
                #endregion
                _client.DataReceived += (sender, e) =>
                {
                    string response = e.GetReceived();
                    Response _r = JsonConvert.DeserializeObject<Response>(response);
                    LobbyResponseHandler[_r.ResponseType](response);
                };

                _client.Send(JsonConvert.SerializeObject(AvailableRoomsRequest.Default));

            }
            HubResponseHandler.Add(typeof(AvailableLobbiesResponse).ToString(), availableLobbyHandler);
            #endregion

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
            while (true) ;
        }
    }
}
