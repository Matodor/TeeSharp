using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TeeSharp
{
    public class IStorage : ISingleton
    {
        public enum StorageType
        {
            BASIC = 0,
            SERVER = 1,
            CLIENT = 2
        }

        public const int
            TYPE_SAVE = 0,
            TYPE_ALL = -1;
    }
}
