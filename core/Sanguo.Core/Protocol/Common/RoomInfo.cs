namespace Sanguo.Core.Protocol.Lobby
{
    public class RoomInfo
    {
        public string Identity { get; set; }
        public int MaxPlayers { get; set; }
        public int JoinedPlayers { get; set; }
        public int RoomPort { get; set; }
    }
}
