using System;
using System.Collections.Generic;

namespace TeeSharp.Common
{
    public class Kernel : IKernel
    {
        private readonly Dictionary<Type, Binder> _binders;

        public Kernel(IKernelConfig config)
        {
            BaseInterface.Kernel = this;
            _binders = new Dictionary<Type, Binder>();
            config.Load(this);
        }

        public T Get<T>() where T : BaseInterface
        {
            if (_binders.TryGetValue(typeof(T), out var binder))
                return (T) binder.Activator();
            throw new Exception($"Type '{typeof(T).Name}' is not binded");
        }

        public Binder<T> Bind<T>() where T : BaseInterface
        {
            var binder = new Binder<T>();
            _binders.Add(binder.BindedType, binder);
            return binder;
        }
    }
}