using Sanguo.Core;
using System;

namespace Sanguo.Lobby
{
    class LobbyRequestsHandler : ISanguoPlugin
    {
        public void OnAnyLoadedCommon()
        {
            // this method should never be called.
            throw new NotImplementedException();
        }

        public void OnClientLoadedOnly()
        {
            // this method should never be called.
            throw new NotImplementedException();
        }

        public void OnServerLoadedOnly()
        {
        }
    }
}
