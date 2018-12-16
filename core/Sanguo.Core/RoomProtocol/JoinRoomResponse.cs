using System;
using System.Collections.Generic;
using System.Text;
using Sanguo.Core.Protocol;
namespace Sanguo.Core.RoomProtocol
{
    public class JoinRoomResponse:Response
    {
        public int RoomPort { get; set; }
    }
}
