using Sanguo.Core.Communication;
using Sanguo.Core.Protocol.Lobby;

namespace Sanguo.Lobby
{
    public class Room
    {
        public RoomInfo RoomInfo { get; set; }
        private readonly IOCPServer _server; 
        public Room(int roomPort, string identity)
        {
            RoomInfo = new RoomInfo
            {
                Identity = identity,
                RoomPort = roomPort,
                MaxPlayers = 5,
                JoinedPlayers = 0
            };
            _server = new IOCPServer(roomPort, 5);
            _server.Init();
            _server.Start();
            
        }
    }
}
