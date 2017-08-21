using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TeeSharp
{
    public enum StorageType
    {
        BASIC = 0,
        SERVER = 1,
        CLIENT = 2
    }

    public interface IStorage
    {
    }
}
