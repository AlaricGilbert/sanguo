namespace Sanguo.ConsoleClient
{
    class Program
    {
        static void Main(string[] args)
        {
            //Run a inside-client server to simplify debug workflow
#if DEBUG
            HubServer.Hub.Run();
#endif
            Client.Run();
        }
    }
}
