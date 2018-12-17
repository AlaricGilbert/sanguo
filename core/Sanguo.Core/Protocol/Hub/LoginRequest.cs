using Sanguo.Core.Protocol.Common;

namespace Sanguo.Core.Protocol.Hub
{
    public class LoginRequest:Request
    {
        public string Username { get; set; }
        public string HashedPw { get; set; }
    }
}
