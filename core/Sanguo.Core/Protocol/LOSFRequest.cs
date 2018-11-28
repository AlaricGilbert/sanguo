using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace Sanguo.Core.Protocol
{
    /// <summary> 
    /// Lobby online status refresh request.
    /// </summary>
    public class LOSFRequest:Request
    {
        [JsonIgnore]
        public const string MagicString = "luojiatql";
        [JsonIgnore]
        public static readonly Response WrongMagicStringResponse = new Response
        {
            ResponseMessage = "Wrong magic string, communication initialization failed.",
            StateNumber = ResponseStates.LOSFVerifyFailed,
            Status = false
        };
        [JsonIgnore]
        public static readonly Response LOSFFinishedResponse = new Response
        {
            ResponseMessage = "LOSF operation succeeded.",
            StateNumber = ResponseStates.LOSFSucceeded,
            Status = true
        };
        public string IPAddress { get; set; }
        public int Port { get; set; }
        public string ServerIdentifier { get; set; }
    }
}
