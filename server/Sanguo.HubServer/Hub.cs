using Newtonsoft.Json;
using Sanguo.Core.Communication;
using Sanguo.Core.Protocol;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Sockets;

namespace Sanguo.HubServer
{
    public static class Hub
    {
        public const int Port = 18112;
        public const int MaxClient = 1000;
        static IOCPServer hubServer;

        private static readonly Dictionary<string, RequestHandler> requestHandlers = new Dictionary<string, RequestHandler>();

        public delegate void RequestHandler
            (string jsonRequest, IOCPServer server, SocketAsyncEventArgs args);

        public static void AddRequestHandler(string requestType, RequestHandler handler) => requestHandlers.TryAdd(requestType, handler);
        public static void HandleRequest(string requestType, string jsonRequest, IOCPServer server, SocketAsyncEventArgs args) => requestHandlers[requestType].Invoke(jsonRequest, server, args);

        public const string UserDatabaseFileName = "userdata.db";
        public static readonly string RunningPath = Environment.CurrentDirectory;
        public static readonly LoginDatabase LoginDB = new LoginDatabase(Path.Combine(RunningPath, UserDatabaseFileName));

        public static readonly Dictionary<string, string> SessionIDs = new Dictionary<string, string>();

        public static void Run()
        {
            #region Plugin
            List<IHubPlugin> hubPlugins = new List<IHubPlugin>();
            hubPlugins.Add(new LoginServer());
            //Todo: load out-side plugins

            foreach (var plugin in hubPlugins)
                plugin.OnLoad();
            #endregion

            Hub.LoginDB.Open();

            #region Server startion
            hubServer = new IOCPServer(Port, MaxClient);
            hubServer.DataReceived += (sender, e) =>
            {
                try
                {
                    string jsonRequest = e.GetReceived();
                    Request r = JsonConvert.DeserializeObject<Request>(jsonRequest);
                    Hub.HandleRequest(r.RequestType, jsonRequest, (IOCPServer)sender, e);
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex);
                }
            };
            hubServer.Init();
            hubServer.Start();
            #endregion
        }
    }
}
