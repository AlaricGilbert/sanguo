using Newtonsoft.Json;
using Sanguo.Core.Communication;
using Sanguo.Core.Protocol;
using System;
using System.Collections.Generic;

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
        }
    }
}
