using Sanguo.Core.Protocol.Common;

namespace Sanguo.Core.Protocol.Lobby
{
    public class JoinRoomRequest //this request should be sent to lobby.
    :Request
    {
        public string RoomIdentity { get; set; }
    }
}
