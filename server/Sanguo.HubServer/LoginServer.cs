using Newtonsoft.Json;
using Sanguo.Core;
using Sanguo.Core.Protocol;
using System;

namespace Sanguo.HubServer
{
    class LoginServer : ISanguoPlugin
    {
        public void OnServerLoaded()
        {
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
                        StateNumber = ResponseStates.LoginSucceeded,
                        ResponseType = typeof(LoginResponse).ToString()
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
                        StateNumber = ResponseStates.LoginVerifyFailed,
                        ResponseType = typeof(LoginResponse).ToString()
                    };
                    server.Send(args, JsonConvert.SerializeObject(response));
                }
            };
            Hub.AddRequestHandler(typeof(LoginRequest).ToString(), loginHandler);
        }
        public void OnClientLoaded() { }
    }
}
