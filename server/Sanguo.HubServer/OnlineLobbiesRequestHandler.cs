﻿using Newtonsoft.Json;
using Sanguo.Core.Communication;
using Sanguo.Core;
using System;
using Sanguo.Core.Protocol.Lobby;
using Sanguo.Core.Protocol.Hub;
using Sanguo.Core.Protocol.Common;

namespace Sanguo.HubServer
{
    class OnlineLobbiesRequestHandler: ISanguoPlugin
    {
        public void OnServerLoadedOnly()
        {
            void lobbyFreshHandler(string json, IOCPServer server, System.Net.Sockets.SocketAsyncEventArgs args)
            {
                LOSFRequest request = JsonConvert.DeserializeObject<LOSFRequest>(json);
                if (request.RequestMessage != LOSFRequest.MagicString)
                    server.Send(args, JsonConvert.SerializeObject(LOSFRequest.WrongMagicStringResponse));
                else
                {
                    server.Send(args, JsonConvert.SerializeObject(LOSFRequest.LOSFFinishedResponse));
                    if (!Hub.LobbyInfos.ContainsKey(request.ServerIdentifier))
                        Hub.LobbyInfos.Add(request.ServerIdentifier, new LobbyInfo
                        {
                            IP = request.IPAddress,
                            Port = request.Port,
                            Identyfier = request.ServerIdentifier
                        });
                }
            }
            Hub.AddRequestHandler(typeof(LOSFRequest).ToString(), lobbyFreshHandler);
            void availableLobbiesHandler(string json, IOCPServer server, System.Net.Sockets.SocketAsyncEventArgs args)
            {
                server.Send(args, JsonConvert.SerializeObject(new AvailableLobbiesResponse
                {
                    ResponseMessage = "Operation finished correctly",
                    StateNumber = ResponseStates.Succeeded,
                    Status = true,
                    LobbyInfos = Hub.LobbyInfos,
                    ResponseType = typeof(AvailableLobbiesResponse).ToString()
                }));
            }
            Hub.AddRequestHandler(typeof(AvailableLobbiesRequest).ToString(), availableLobbiesHandler);
        }

        public void OnClientLoadedOnly()
        {
            throw new NotImplementedException();
        }

        public void OnAnyLoadedCommon()
        {
            throw new NotImplementedException();
        }
    }
}
