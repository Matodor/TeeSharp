using TeeSharp.Core;
using TeeSharp.Server;
using TeeSharp.Server.Game;

namespace Examples.BasicServer
{
    internal class Program
    {
        internal static void Main(string[] args)
        {
            var kernel = new Kernel(new ServerKernelConfig());
            var server = kernel.Get<BaseServer>();
            server.Init(args);
            server.AddGametype<GameControllerDM>("DM");
            server.AddGametype<GameControllerCTF>("CTF");
            server.AddGametype<GameControllerMod>("MOD");
            server.Run();
        }
    }
}