using Sanguo.Core.Protocol.Common;
using Sanguo.Core.Protocol.Lobby;
using System.Collections.Generic;

namespace Sanguo.Core.Protocol.Lobby
{
    public class AvailableRoomsResponse:Response
    {
        public List<RoomInfo> RoomInfos { get; set; }
    }
}
