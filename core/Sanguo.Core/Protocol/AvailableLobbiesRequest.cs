using Newtonsoft.Json;

namespace Sanguo.Core.Protocol
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
