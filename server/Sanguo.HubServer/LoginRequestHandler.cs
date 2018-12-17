using Newtonsoft.Json;
using Sanguo.Core;
using Sanguo.Core.Communication;
using Sanguo.Core.Protocol.Common;
using Sanguo.Core.Protocol.Hub;
using System;
using System.Net.Sockets;

namespace Sanguo.HubServer
{
    class LoginRequestHandler : ISanguoPlugin
    {
        public void OnServerLoadedOnly()
        {
            async void loginHandler(string json, IOCPServer server, SocketAsyncEventArgs args)
            {
                LoginRequest request = JsonConvert.DeserializeObject<LoginRequest>(json);
                if (await Hub.LoginDB.LoginAsync(request))
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
            }
            Hub.AddRequestHandler(typeof(LoginRequest).ToString(), loginHandler);
        }
        public void OnClientLoadedOnly()
        {
            throw new NotImplementedException();// this method should never be called.
        }
        public void OnAnyLoadedCommon()
        {
            throw new NotImplementedException();// this method should never be called.
        }
    }
}
