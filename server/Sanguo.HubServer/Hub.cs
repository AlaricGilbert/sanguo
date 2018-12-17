using Newtonsoft.Json;
using Sanguo.Core;
using Sanguo.Core.Communication;
using Sanguo.Core.Protocol.Common;
using Sanguo.Core.Protocol.Lobby;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Sockets;

namespace Sanguo.HubServer
{
    public static class Hub
    {
        public static Logger HubLogger = new Logger("hub");

        #region Hub IOCP fields.
        public const int Port = 18112;
        public const int MaxClient = 1000;
        private static IOCPServer _hubServer;
        #endregion

        #region Hub Request Handlers
        private static readonly Dictionary<string, RequestHandler> requestHandlers = new Dictionary<string, RequestHandler>();
        public delegate void RequestHandler
            (string jsonRequest, IOCPServer server, SocketAsyncEventArgs args);
        public static void AddRequestHandler(string requestType, RequestHandler handler) => requestHandlers.TryAdd(requestType, handler);
        public static void HandleRequest(string requestType, string jsonRequest, IOCPServer server, SocketAsyncEventArgs args) => requestHandlers[requestType].Invoke(jsonRequest, server, args);
        #endregion

        #region Login Servers
        public const string UserDatabaseFileName = "userdata.db";
        public static readonly string RunningPath = Environment.CurrentDirectory;
        public static readonly LoginDatabase LoginDB = new LoginDatabase(Path.Combine(RunningPath, UserDatabaseFileName));
        public static readonly Dictionary<string, string> SessionIDs = new Dictionary<string, string>();
        #endregion

        #region Lobby Servers
        public static Dictionary<string, LobbyInfo> LobbyInfos = new Dictionary<string, LobbyInfo>();
        #endregion

        #region Message Hook
        public delegate void MessageHandler(string m);
        public static event MessageHandler MessageReceived;
        #endregion

        public static void Run()
        {
            #region Initialize hub plugs
            List<ISanguoPlugin> hubPlugins = new List<ISanguoPlugin>
            {
                new LoginRequestHandler(),
                new OnlineLobbiesRequestHandler()
            };
            //Todo: load out-side plugins

            //Handshake request handler..
            RequestHandler hsHandler = (json, server, args) =>
            {
                HandshakeRequest request = JsonConvert.DeserializeObject<HandshakeRequest>(json);
                if (request.RequestMessage == HandshakeRequest.MagicMessage)
                    server.Send(args, JsonConvert.SerializeObject(HandshakeResponse.Default));
                else
                    server.Send(args, JsonConvert.SerializeObject(HandshakeResponse.WrongMagicMessage));
            };
            AddRequestHandler(typeof(HandshakeRequest).ToString(), hsHandler);

            //Load all plugins.
            foreach (var plugin in hubPlugins)
                plugin.OnServerLoadedOnly();
            #endregion

            LoginDB.Open();

            #region Server startion
            _hubServer = new IOCPServer(Port, MaxClient);
            _hubServer.DataReceived += (sender, e) =>
            {
                string jsonRequest = "";
                try
                {
                    jsonRequest = e.GetReceived();
                    Request r = JsonConvert.DeserializeObject<Request>(jsonRequest);
                    HandleRequest(r.RequestType, jsonRequest, (IOCPServer)sender, e);
                }
                catch (Exception ex)
                {
                    HubLogger.Write(ex);
                }
                finally
                {
                    MessageReceived(jsonRequest);
                }
            };
            _hubServer.Init();
            _hubServer.Start();
            #endregion
        }
    }
}
