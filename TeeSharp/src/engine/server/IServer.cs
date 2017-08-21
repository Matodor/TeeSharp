using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TeeSharp.Server
{
    public interface IServer
    {
        void Run();
        void Init(string[] args);

        void RegisterCommands();
        bool LoadMap(string mapName);
    }
}
