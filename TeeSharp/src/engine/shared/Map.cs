using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TeeSharp
{
    public class Map : IEngineMap
    {
        protected Map()
        {
            
        }

        public static Map Create()
        {
            return new Map();
        }
    }
}
