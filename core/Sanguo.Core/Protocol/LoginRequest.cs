namespace Sanguo.Core.Protocol
{
    public class LoginRequest:Request
    {
        public string Username { get; set; }
        public string HashedPw { get; set; }
    }
}
