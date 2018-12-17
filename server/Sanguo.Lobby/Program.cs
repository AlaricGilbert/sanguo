using System;

namespace Sanguo.Lobby
{
    class Program
    {
        static void Main(string[] args)
        {
            int port = 10023;
            if (args.Length == 2)
            {
                if (args[0] == "-p")
                {
                    port = Convert.ToInt32(args[1]);
                }
            }
            Lobby.Init(port, 5 * 1000);
            Lobby.Run();
        }
    }
}
