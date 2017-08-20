using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TeeSharp
{
    public class Storage : IStorage
    {
        protected Storage(string applicationName, StorageType storageType)
        {
            
        }

        public static Storage Create(string applicationName, StorageType storageType)
        {
            return new Storage(applicationName, storageType);
        }
    }
}
