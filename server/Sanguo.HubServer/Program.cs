namespace Sanguo.HubServer
{
    class Program
    {
        static void Main(string[] args)
        {
            Hub.MessageReceived += (string m) => Hub.HubLogger.Write(m, "HubServer/DataReceived", Core.Logger.LogLevel.Infos);
            Hub.Run();
        }
    }
}
