namespace Sanguo.ConsoleClient
{
    class Program
    {
        public static Core.Logger logger = new Core.Logger("client");
        static void Main(string[] args)
        {
            //Run a inside-client server to simplify debug workflow

#if DEBUG
            HubServer.Hub.Run();
            HubServer.Hub.MessageReceived += (string m) => logger.Write(m, "ConsoleClient/MainMethod", Core.Logger.LogLevel.Infos);
            Lobby.Lobby lobby = new Lobby.Lobby(123, 1000);
            lobby.Run();
#endif
            Client.Run();
        }
    }
}
