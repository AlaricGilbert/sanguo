using System;
using System.Collections.Generic;
using System.Text;
using Sanguo.Core.Communication;

namespace Sanguo.HubServer
{
    static class HubServer
    {
        public const int Port = 18112;
        public const int MaxClient = 1000;
        static IOCPServer hubServer;
        public static void Start()
        {
            hubServer = new IOCPServer(Port, MaxClient);
            
        }
    }
}
