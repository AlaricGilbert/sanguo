using Sanguo.Core.Communication;
using System.Collections.Generic;
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
    }
}
