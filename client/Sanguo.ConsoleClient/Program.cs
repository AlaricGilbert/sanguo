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
            string dir = Path.Combine("..", "..", "..", "..", "..", "server", "srvn", "bin", "Debug", "netcoreapp2.1", "srvn.exe");
            Process _hubProcess = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = dir.Replace("srvn", "Sanguo.HubServer"),
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
                    FileName = dir.Replace("srvn", "Sanguo.Lobby"),
                    Arguments = "-p 10022",
                    RedirectStandardError = false,
                    RedirectStandardInput = false,
                    RedirectStandardOutput = false,
                    CreateNoWindow = false
                }
            };
            _hubProcess.Start();//
            Thread.Sleep(100);//wait the hub init.
            _lobbyProcess.Start();
            Thread.Sleep(100);//wait the lobby and hub connect.
#endif
            Client.Run();
        }
    }
}
