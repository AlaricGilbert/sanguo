using System.Collections.Generic;
using Sanguo.Core.Protocol.Common;
using Sanguo.Core.Protocol.Lobby;

namespace Sanguo.Core.Protocol.Hub
{
    public class AvailableLobbiesResponse:Response
    {
        public Dictionary<string, LobbyInfo> LobbyInfos { get; set; }
    }
}
