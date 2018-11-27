namespace Sanguo.Core.Protocol
{
    public class HandshakeResponse:Response
    {
        public string ServerName { get; set; }
        public int ProtocolVersion { get; set; }
        public static readonly HandshakeResponse Default = new HandshakeResponse {
            ServerName = "Sanguo server",
            ProtocolVersion = 1,
            StateNumber = ResponseStates.HandshakeSucceeded,
            Status = true,
            ResponseMessage = "HandshakeRequest finished successfully."
        };
        public static readonly HandshakeResponse WrongMagicMessage = new HandshakeResponse
        {
            ServerName = "Sanguo server",
            ProtocolVersion = 1,
            StateNumber = ResponseStates.HandshakeVerifyFailed,
            Status = false,
            ResponseMessage = "Wrong magic message."
        };
    }
}
