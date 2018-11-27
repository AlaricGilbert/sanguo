using Newtonsoft.Json;
using Sanguo.Core.Protocol;
using System;

namespace Sanguo.HubServer
{
    class LoginServer : IHubPlugin
    {
        public void OnLoad()
        {
            Hub.RequestHandler hsHandler = (json, server, args) =>
            {
                HandshakeRequest request = JsonConvert.DeserializeObject<HandshakeRequest>(json);
                if (request.RequestMessage == HandshakeRequest.MagicMessage)
                    server.Send(args, JsonConvert.SerializeObject(HandshakeResponse.Default));
                else
                    server.Send(args, JsonConvert.SerializeObject(HandshakeResponse.WrongMagicMessage));
            };
            Hub.AddRequestHandler(typeof(HandshakeRequest).ToString(), hsHandler);

            Hub.RequestHandler loginHandler = async (json, server, args) =>
            {
                LoginRequest request = JsonConvert.DeserializeObject<LoginRequest>(json);
                if(await Hub.LoginDB.LoginAsync(request))
                {
                    string sessionID = Guid.NewGuid().ToString();
                    LoginResponse response = new LoginResponse
                    {
                        SessionID = sessionID,
                        ResponseMessage = "Log-in operation succeeded.",
                        Status = true,
                        StateNumber = ResponseStates.LoginSucceeded
                    };
                    server.Send(args, JsonConvert.SerializeObject(response));
                    if (Hub.SessionIDs.ContainsKey(request.Username))
                        Hub.SessionIDs[request.Username] = sessionID;
                    else
                        Hub.SessionIDs.Add(request.Username, sessionID);
                }
                else
                {
                    LoginResponse response = new LoginResponse
                    {
                        SessionID = LoginResponse.LoginFailedMagicSessionID,
                        ResponseMessage = "Wrong username or password, or the account does not exist.",
                        Status = false,
                        StateNumber = ResponseStates.LoginVerifyFailed
                    };
                    server.Send(args, JsonConvert.SerializeObject(response));
                }
            };
            Hub.AddRequestHandler(typeof(LoginRequest).ToString(), loginHandler);
        }
    }
}
