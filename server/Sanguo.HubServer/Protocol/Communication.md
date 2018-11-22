# Communication examples between Server&Client

## Handshake
Client->Server Request:
```json
{
    "RequestMessage":"jachin server?"
}
```
A possible Server->Client Result:
```json
{
    "ServerName":"yet another jachin server.",
    "ProtocolVersion":1,
    "StateNumber":101,
    "Status":true,
    "ResultMessage":"HandshakeRequest finished successfully."
}
```

## Log-in
Client -> Server Request:
```json
{
    "RequestMessage":"login request",
    "Username":"maniacata",
    "Password":"maniacata_pw"
}
```
A possible Server->Client Result:
```json
{
    "SessionID":"1324c819-ffbc-406c-84a8-320e38b49466",
    "StateNumber":102,
    "Status":true,
    "ResultMessage":"Log-in successfully."
}
```

## Register
Client -> Server Request:
```json
{
    "RequestMessage":"register request",
    "Username":"maniacata",
    "Password":"maniacata_pw",
    "AvatarUri":"1.ico"
}
```
A possible Server -> Client Result:
```json
{
    "StateNumber":103,
    "Status":true,
    "ResultMessage":"Successfully registered."
}
```
Another possible Server -> Client Result:
```json
{
    "StateNumber":203,
    "Status":false,
    "ResultMessage":"The username has been occupied."
}
```

## Result State defination.
```cs
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
```