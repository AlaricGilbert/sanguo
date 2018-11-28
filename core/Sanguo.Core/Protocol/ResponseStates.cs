namespace Sanguo.Core.Protocol
{
    public static class ResponseStates
    {
        public const int Succeeded = 1000;
        public const int HandshakeSucceeded = Succeeded + 1;
        public const int LoginSucceeded = Succeeded + 2;
        public const int RegisterSucceeded = Succeeded + 3;
        public const int LOSFSucceeded = Succeeded + 4;

        public const int FormatError = 2000;

        public const int VerifyFailed = 3000;
        public const int HandshakeVerifyFailed = VerifyFailed + 1;
        public const int LoginVerifyFailed = VerifyFailed + 2;
        public const int LOSFVerifyFailed = VerifyFailed + 4;

        public const int Occupied = 4000;
        public const int RegisterAccountOccupied = Occupied + 3;
    }
}
