using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Teecsharp
{
    public abstract class IKernel : IDisposable
    {
        public abstract bool RegisterInterfaceImpl(string InterfaceName, IInterface pInterface);
        public abstract bool ReregisterInterfaceImpl(string InterfaceName, IInterface pInterface);
        public abstract T RequestInterfaceImpl<T>(string InterfaceName) where T : IInterface;

        private string NameOfInterface(IInterface pInterface)
        {
            Type baseType = pInterface.GetType();
            while (baseType != null && baseType.BaseType != null)
            {
                if (baseType.BaseType != typeof(IInterface))
                    baseType = baseType.BaseType;
                else break;
            }
            return baseType.Name;
        }

        public bool RegisterInterface(IInterface pInterface)
        {
            return RegisterInterfaceImpl(NameOfInterface(pInterface), pInterface);
        }

        public bool ReregisterInterface(IInterface pInterface)
        {
            return ReregisterInterfaceImpl(NameOfInterface(pInterface), pInterface);
        }

        public T RequestInterface<T>() where T : IInterface
        {
            Type type = typeof(T);
            while (type?.BaseType != null)
            {
                if (type.BaseType != typeof(IInterface))
                    type = type.BaseType;
                else break;
            }
            return RequestInterfaceImpl<T>(type.Name);
        }

        public void Dispose()
        {
            throw new NotImplementedException();
        }
    }
}
