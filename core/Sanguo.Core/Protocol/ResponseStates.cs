namespace Sanguo.Core.Protocol
{
    public static class ResponseStates
    {
        public const int Successful = 1000;
        public const int HandshakeFinished = Successful + 1;

        public const int FormatError = 2000;

        public const int VerifyFailed = 3000;
        public const int HandshakeVerifyFailed = VerifyFailed + 1;
    }
}
