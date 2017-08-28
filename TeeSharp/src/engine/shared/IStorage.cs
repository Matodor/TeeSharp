using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TeeSharp
{
    public interface IStorage
    {
        void Init(string applicationName);

        Stream OpenFile(string path, FileMode mode, FileAccess access, FileShare share);
    }
}
