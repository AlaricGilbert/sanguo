using Newtonsoft.Json;
using Sanguo.Core;
using Sanguo.Core.Communication;
using Sanguo.Core.Protocol.Common;
using Sanguo.Core.Protocol.Lobby;
using System;
using System.Net.Sockets;

namespace Sanguo.Lobby
{
    class LobbyRequestsHandler : ISanguoPlugin
    {
        public void OnAnyLoadedCommon()
        {
            // this method should never be called.
            throw new NotImplementedException();
        }

        public void OnClientLoadedOnly()
        {
            // this method should never be called.
            throw new NotImplementedException();
        }

        public void OnServerLoadedOnly()
        {
            void availableRoomsHandler(string json, IOCPServer server, SocketAsyncEventArgs args)
            {
                server.Send(args, JsonConvert.SerializeObject(new AvailableRoomsResponse
                {
                    ResponseMessage = "Operation finished correctly",
                    StateNumber = ResponseStates.Succeeded,
                    Status = true,
                    RoomInfos = Lobby.RoomInfos,
                    ResponseType = typeof(AvailableRoomsResponse).ToString()
                }));
            }
            Lobby.AddRequestHandler(typeof(AvailableRoomsRequest).ToString(),availableRoomsHandler);
        }
    }
}
