using Newtonsoft.Json;
using Sanguo.Core.Protocol.Common;

namespace Sanguo.Core.Protocol.Lobby
{
    public class AvailableRoomsRequest : Request
    {
        [JsonIgnore]
        public const string MagicString = "rooms?";
        [JsonIgnore]
        public static readonly AvailableRoomsRequest Default = new AvailableRoomsRequest
        {
            RequestMessage = MagicString,
            RequestType = typeof(AvailableRoomsRequest).ToString()
        };
    }
}
