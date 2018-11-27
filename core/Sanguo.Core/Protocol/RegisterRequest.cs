using System;
using System.Collections.Generic;
using System.Text;

namespace Sanguo.Core.Protocol
{
    public class RegisterRequest :Request
    {
        public string Username { get; set; }
        public string HashedPw { get; set; }
        public string AvatarUri { get; set; }
    }
}
