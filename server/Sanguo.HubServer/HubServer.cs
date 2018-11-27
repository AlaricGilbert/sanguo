using Newtonsoft.Json;
using Sanguo.Core.Protocol;
using System;

namespace Sanguo.HubServer
{
    class HubServer : IHubPlugin
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
                    LoginResponse response = new LoginResponse
                    {
                        SessionID = Guid.NewGuid().ToString(),
                        ResponseMessage = "Log-in operation succeeded.",
                        Status = true,
                        StateNumber = ResponseStates.LoginSucceeded
                    };
                    server.Send(args, JsonConvert.SerializeObject(response));
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
