using Newtonsoft.Json;
using Sanguo.Core.Protocol;
using Sanguo.Core.Communication;
using System.Net;

namespace Sanguo.HubServer
{
    class LobbyHoster: IHubPlugin
    {
        public void OnLoad()
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
                    LobbyInfos = Hub.LobbyInfos
                }));
            }
            Hub.AddRequestHandler(typeof(AvailableLobbiesRequest).ToString(), availableLobbiesHandler);
        }
    }
}
