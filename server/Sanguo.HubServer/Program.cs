using Newtonsoft.Json;
using Sanguo.Core.Communication;
using Sanguo.Core.Protocol;
using System;
using System.Collections.Generic;
using System.Net;

namespace Sanguo.HubServer
{
    class Program
    {

        public const int Port = 18112;
        public const int MaxClient = 1000;
        static IOCPServer hubServer;

        static void Main(string[] args)
        {
            #region Plugin
            List<IHubPlugin> hubPlugins = new List<IHubPlugin>();
            hubPlugins.Add(new HubServer());
            //Todo: load out-side plugins

            foreach (var plugin in hubPlugins)
                plugin.OnLoad();
            #endregion

            #region Server startion
            hubServer = new IOCPServer(Port, MaxClient);
            hubServer.DataReceived += (sender, e) =>
            {
                try
                {
                    string jsonRequest = e.GetReceived();
                    Request r = JsonConvert.DeserializeObject<Request>(jsonRequest);
                    Hub.HandleRequest(r.RequestType, jsonRequest, (IOCPServer)sender, e);
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex);
                }
            };
            hubServer.Init();
            hubServer.Start();
            #endregion
        }
    }
}
