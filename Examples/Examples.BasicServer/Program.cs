using TeeSharp.Core;
using TeeSharp.Server;

namespace Examples.BasicServer
{
    internal class Program
    {
        internal static void Main(string[] args)
        {
            var kernel = new Kernel(new DefaultServerKernel());
            var server = kernel.Get<BaseServer>();
            server.Init(args);
            server.Run();
        }
    }
}