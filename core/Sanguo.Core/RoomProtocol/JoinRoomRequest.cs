using System;
using System.Collections.Generic;
using System.Text;
using Sanguo.Core.Protocol;
namespace Sanguo.Core.RoomProtocol
{
    public class JoinRoomRequest //this request should be sent to lobby.
    :Request
    {
        public string RoomIdentity { get; set; }
    }
}
