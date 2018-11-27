using Newtonsoft.Json;
using Sanguo.Core.Communication;
using Sanguo.Core.Protocol;
using Sanguo.Core.Data;
using System;
using System.Collections.Generic;
using System.Net;
using System.Threading;

namespace Sanguo.HubServer
{
    class Program
    {

        public const int Port = 18112;
        public const int MaxClient = 1000;
        static IOCPServer hubServer;
        static bool waiting = true;
        static void Main(string[] args)
        {
            RunServerTest();
            RunClientTest();
        }
        static void RunServerTest()
        {

            #region Plugin
            List<IHubPlugin> hubPlugins = new List<IHubPlugin>();
            hubPlugins.Add(new HubServer());
            //Todo: load out-side plugins

            foreach (var plugin in hubPlugins)
                plugin.OnLoad();
            #endregion

            Hub.LoginDB.Open();

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
        static void RunClientTest()
        {
            IOCPClient client = new IOCPClient(IPAddress.Parse("127.0.0.1"), 18112);
            client.DataReceived += (sender, e) => {
                string info = e.GetReceived();
                Console.WriteLine(info);
                waiting = false;
            };
            client.Connect();
            client.Listen();


            LoginRequest loginRequest = new LoginRequest
            {
                Username = "alaric",
                HashedPw = "666",
                RequestType = typeof(LoginRequest).ToString()
            };

            string s = JsonConvert.SerializeObject(loginRequest);
            Console.WriteLine(s);
            client.Send(s);

            while (waiting) { }
            
            LoginRequest loginRequest1 = new LoginRequest
            {
                Username = "alaric",
                HashedPw = "777",
                RequestType = typeof(LoginRequest).ToString()
            };

            string sp = JsonConvert.SerializeObject(loginRequest1);
            Console.WriteLine(sp);
            client.Send(sp);
        }
        static void GenerateDatabase()
        {
            Hub.LoginDB.Open();
            Hub.LoginDB.RunCommand(string.Format(SQLiteCommands.CreateUserTable));
            Hub.LoginDB.RegisterAsync(new RegisterRequest { Username = "alaric", AvatarUri = "1.ico", HashedPw = "666" });
        }
    }
}
