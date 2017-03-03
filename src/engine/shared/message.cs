using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Teecsharp
{
    public class CMsgPacker : CPacker
    {
        public CMsgPacker(int Type)
        {
            Reset();
            AddInt(Type);
        }
    }
}
