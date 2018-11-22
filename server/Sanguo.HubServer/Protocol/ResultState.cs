namespace Sanguo.HubServer.Protocol
{
    public enum ResultState
    {
        Successful = 100,
        HandshakeFinished = 101,
        LoginSuccessfully = 102,
        RegisterFinished = 103,
        Failed = 200,
        UserNotExist = 201,
        WrongPassword = 202,
        UsernameOccupied = 203,
        InvalidRequestFormat = 300
    }
}
