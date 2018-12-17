using System.Collections.Generic;
using Sanguo.Core.Communication;
using Sanguo.Core.Protocol.Common;

namespace Sanguo.Core.Protocol.Hub
{
    public class AvailableLobbiesResponse:Response
    {
        public Dictionary<string, LobbyInfo> LobbyInfos { get; set; }
    }
}
