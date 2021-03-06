﻿using Newtonsoft.Json;
using Sanguo.Core.Protocol.Common;

namespace Sanguo.Core.Protocol.Lobby
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
            Status = false,
            ResponseType = "LOSFResponse"
        };
        [JsonIgnore]
        public static readonly Response LOSFFinishedResponse = new Response
        {
            ResponseMessage = "LOSF operation succeeded.",
            StateNumber = ResponseStates.LOSFSucceeded,
            Status = true,
            ResponseType = "LOSFResponse"
        };
        public string IPAddress { get; set; }
        public int Port { get; set; }
        public string ServerIdentifier { get; set; }
    }
}
