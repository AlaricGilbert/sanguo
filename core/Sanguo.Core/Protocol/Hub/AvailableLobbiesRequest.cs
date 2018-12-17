using Newtonsoft.Json;
using Sanguo.Core.Protocol.Common;

namespace Sanguo.Core.Protocol.Hub
{
    public class AvailableLobbiesRequest : Request
    {
        [JsonIgnore]
        public const string MagicString = "lobbies?";
        [JsonIgnore]
        public static readonly AvailableLobbiesRequest Default = new AvailableLobbiesRequest
        {
            RequestMessage = MagicString,
            RequestType = typeof(AvailableLobbiesRequest).ToString()
        };
    }
}
