using System.Diagnostics;
using System.Threading;
using System.IO;

namespace Sanguo.ConsoleClient
{
    class Program
    {
        public static Core.Logger logger = new Core.Logger("client");
        static void Main(string[] args)
        {
            //Run a built-in server to simplify the debug workflow.
#if DEBUG
            Process _hubProcess = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = "Sanguo.HubServer.exe",
                    Arguments = "",
                    RedirectStandardError = false,
                    RedirectStandardInput = false,
                    RedirectStandardOutput = false,
                    CreateNoWindow = false
                }
            };
            Process _lobbyProcess = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = "Sanguo.Lobby.exe",
                    Arguments = "-p 10022",
                    RedirectStandardError = false,
                    RedirectStandardInput = false,
                    RedirectStandardOutput = false,
                    CreateNoWindow = false
                }
            };
            _hubProcess.Start();
            _lobbyProcess.Start();
#endif
            Client.Run();
        }
    }
}
