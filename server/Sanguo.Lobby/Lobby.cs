using Newtonsoft.Json;
using Sanguo.Core;
using Sanguo.Core.Communication;
using Sanguo.Core.Protocol;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace Sanguo.Lobby
{
    public class Lobby
    {
        private const string HubAddr = "127.0.0.1";
        private const int HubPort = 18112;
        public int Port { get; internal set; }
        public int MaxClients { get; internal set; }
        public delegate void RequestHandler(string jsonRequest, IOCPServer server, SocketAsyncEventArgs args);
        private readonly Dictionary<string, RequestHandler> _lobbyRequestHandlers = new Dictionary<string, RequestHandler>();
        public void AddRequestHandler(string requestType, RequestHandler requestHandler) => _lobbyRequestHandlers.Add(requestType, requestHandler);
        public string LobbyAddr { get; internal set; }
        private IOCPServer _lobbyServer;
        private IOCPClient _hubConnecter;
        public Lobby(int lobbyPort, int maxClients)
        {
            Port = lobbyPort;
            MaxClients = maxClients;
            LobbyAddr = "127.0.0.1";
            _lobbyServer = new IOCPServer(lobbyPort, maxClients);

            _hubConnecter = new IOCPClient(IPAddress.Parse(HubAddr), HubPort);
        }
        public virtual void Run()
        {
            AddLobbyServerHandlers();
            _lobbyServer.Init();
            _lobbyServer.Start();

            _hubConnecter.Connect();
            _hubConnecter.Listen();

            List<ISanguoPlugin> plugins = new List<ISanguoPlugin>
            {
                new LobbyDataHandler()
            };
            //To do : client-side outside plugins load.
            foreach(var plug in plugins)
            {
            }


            LOSFRequest losf = new LOSFRequest
            {
                IPAddress = LobbyAddr,
                Port = Port,
                RequestMessage = LOSFRequest.MagicString,
                RequestType = typeof(LOSFRequest).ToString(),
                ServerIdentifier = "pris01"
            };
            _hubConnecter.Send(JsonConvert.SerializeObject(losf));

        }

        private void AddLobbyServerHandlers()
        {
            
        }
    }
}
