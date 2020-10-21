using System;

namespace TeeSharp.Network
{
    public class NetworkServer : BaseNetworkServer
    {
        public override void Init()
        {
            
        }

        public override void Update()
        {
            
        }

        public override bool Open()
        {
            try
            {
                throw new NotImplementedException();
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
        }
    }
}