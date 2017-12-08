using System;
using System.Collections.Generic;
using System.Text;

namespace TeeSharp
{
    public class Slot
    {
        public readonly NetworkConnection Connection;

        public Slot()
        {
            Connection = new NetworkConnection();
        }
    }
}
