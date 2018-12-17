using Sanguo.Core.Protocol.Common;

namespace Sanguo.Core.Protocol.Hub
{
    public class LoginResponse: Response
    {
        public string SessionID { get; set; }
        public static string LoginFailedMagicSessionID = "69ff9e87-2935-438c-8430-3210da98595a";
    }
}
