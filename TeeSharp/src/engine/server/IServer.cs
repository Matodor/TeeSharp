using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TeeSharp.Server
{
    public interface IServer
    {
        ulong Tick { get; }

        void Run();
        void Init(string[] args);

        Client GetClient(int clientId);

        void RegisterCommands();
        bool LoadMap(string mapName);
    }
}
