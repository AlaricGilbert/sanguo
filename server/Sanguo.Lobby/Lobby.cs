using Newtonsoft.Json;
using Sanguo.Core;
using Sanguo.Core.Communication;
using Sanguo.Core.Protocol.Common;
using Sanguo.Core.Protocol.Lobby;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;

namespace Sanguo.Lobby
{
    public static class Lobby
    {
        private const string HubAddr = "127.0.0.1";
        private const int HubPort = 18112;
        public static int Port { get; internal set; }
        public static int MaxClients { get; internal set; }
        public delegate void RequestHandler(string jsonRequest, IOCPServer server, SocketAsyncEventArgs args);
        private static readonly Dictionary<string, RequestHandler> _lobbyRequestHandlers = new Dictionary<string, RequestHandler>();
        public static void AddRequestHandler(string requestType, RequestHandler requestHandler) => _lobbyRequestHandlers.Add(requestType, requestHandler);
        public static string LobbyAddr { get; internal set; }
        private static IOCPServer _lobbyServer;
        private static IOCPClient _hubConnecter;

        public static List<Room> Rooms = new List<Room>();
        public static List<RoomInfo> RoomInfos = new List<RoomInfo>();
        public static void Init(int lobbyPort, int maxClients)
        {
            Port = lobbyPort;
            MaxClients = maxClients;
            LobbyAddr = "127.0.0.1";
            _lobbyServer = new IOCPServer(lobbyPort, maxClients);

            _hubConnecter = new IOCPClient(IPAddress.Parse(HubAddr), HubPort);
        }
        public static void Run()
        {
            _lobbyServer.Init();
            _lobbyServer.Start();

            _hubConnecter.Connect();
            _hubConnecter.Listen();

            List<ISanguoPlugin> plugins = new List<ISanguoPlugin>
            {
                new LobbyRequestsHandler()
            };
            //[To do : client-side outside plugins load.

            //]
            foreach(var plug in plugins)
            {
                plug.OnServerLoadedOnly();
            }
            _lobbyServer.DataReceived += (sender, e) =>
            {
                string jsonRequest = e.GetReceived();
                Request r = JsonConvert.DeserializeObject<Request>(jsonRequest);
                _lobbyRequestHandlers[r.RequestType](jsonRequest, (IOCPServer)sender, e);
            };


            LOSFRequest losf = new LOSFRequest
            {
                IPAddress = LobbyAddr,
                Port = Port,
                RequestMessage = LOSFRequest.MagicString,
                RequestType = typeof(LOSFRequest).ToString(),
                ServerIdentifier = "pris01"
            };
            _hubConnecter.Send(JsonConvert.SerializeObject(losf));

            //Init 10 rooms.
            for (int i = 0; i < 10; i++)
            {
                Room room = new Room(i + 20000, $"Room_{i}");
                Rooms.Add(room);
                RoomInfos.Add(room.RoomInfo);
            }
            while (true) { }
        }
    }
}
