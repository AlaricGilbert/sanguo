using Newtonsoft.Json;
using Sanguo.Core.Protocol;

namespace Sanguo.HubServer
{
    class LobbyHoster: IHubPlugin
    {
        public void OnLoad()
        {
            Hub.RequestHandler lobbyRefreshRequest = (json, server, args) =>
            {
                LOSFRequest request = JsonConvert.DeserializeObject<LOSFRequest>(json);
                if (request.RequestMessage != LOSFRequest.MagicString)
                {
                    server.Send(args, JsonConvert.SerializeObject(LOSFRequest.WrongMagicStringResponse));
                    
                }
                else
                    server.Send(args, JsonConvert.SerializeObject(LOSFRequest.LOSFFinishedResponse));
            };
        }
    }
}
