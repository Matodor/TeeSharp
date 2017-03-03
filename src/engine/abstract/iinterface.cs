using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Teecsharp
{
    public abstract class IInterface : IDisposable
    {
        protected CConfiguration g_Config { get { return CConfiguration.Instance; } }
        protected IKernel Kernel { get { return m_pKernel; } }

        private IKernel m_pKernel;

        public void SetKernel(IKernel kernel)
        {
            m_pKernel = kernel;
        }

        public void Dispose()
        {
        }
    }
}
