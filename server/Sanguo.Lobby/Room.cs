using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Net;
using System.Text;
using Sanguo.Core.Communication;

namespace Sanguo.Lobby
{
    class Room
    {
        public const int MaxClientCount = 5;
        public int ClientCount { get { return _server.ClientCount; } }
        public int RoomPort { get; }
        public string Identity { get; }
        private readonly IOCPServer _server; 
        public Room(int roomPort, string identity)
        {
            RoomPort = roomPort;
            //identity should be lobby-unique!.
            Identity = identity;
            _server = new IOCPServer(roomPort, MaxClientCount);
            _server.Init();
            _server.Start();
            
        }
    }
}
