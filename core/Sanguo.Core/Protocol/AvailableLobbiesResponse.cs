using System;
using System.Collections.Generic;
using System.Text;
using Sanguo.Core.Communication;

namespace Sanguo.Core.Protocol
{
    public class AvailableLobbiesResponse:Response
    {
        public Dictionary<string, LobbyInfo> LobbyInfos { get; set; }
    }
}
