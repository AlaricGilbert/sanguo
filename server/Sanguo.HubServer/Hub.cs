using Sanguo.Core.Communication;
using Sanguo.Core.Data;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Sockets;

namespace Sanguo.HubServer
{
    public static class Hub
    {
        private static readonly Dictionary<string, RequestHandler> requestHandlers = new Dictionary<string, RequestHandler>();
        public delegate void RequestHandler
            (string jsonRequest, IOCPServer server, SocketAsyncEventArgs args);
        public static void AddRequestHandler(string requestType, RequestHandler handler) => requestHandlers.TryAdd(requestType, handler);
        public static void HandleRequest(string requestType, string jsonRequest, IOCPServer server, SocketAsyncEventArgs args) => requestHandlers[requestType].Invoke(jsonRequest, server, args);
        public const string UserDatabaseFileName = "userdata.db";
        public static readonly string RunningPath = Environment.CurrentDirectory;
        public static readonly LoginDatabase LoginDB = new LoginDatabase(Path.Combine(RunningPath, UserDatabaseFileName));
    }
}
