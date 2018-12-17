using Sanguo.Core.Protocol.Common;

namespace Sanguo.Core.Protocol.Hub
{
    public class RegisterRequest :Request
    {
        public string Username { get; set; }
        public string HashedPw { get; set; }
        public string AvatarUri { get; set; }
    }
}
