using System;
using System.Collections.Generic;

namespace TeeSharp
{
    public abstract class ISingleton
    {
        public Kernel Kernel
        {
            get { return _kernel; }
            set
            {
                if (_kernel == null)
                    _kernel = value;
            }
        }

        private Kernel _kernel;
    }

    public class Kernel
    {
        private readonly Dictionary<Type, ISingleton> _singletons;

        private Kernel()
        {
            _singletons = new Dictionary<Type, ISingleton>();    
        }

        public T RequestSingleton<T>() where T : ISingleton
        {
            var type = typeof(T);
            return _singletons.ContainsKey(type) ? (T) _singletons[type] : null;
        }

        /*public TOut RegisterSingleton<TIn, TOut>(TIn singleton) where TIn : class
                                                                where TOut : class
        {
            var @out = (object) singleton as TOut;
            if (@out == null)
                throw new Exception("Kerner register error");

            var type = typeof(TOut);
            if (_singletons.ContainsKey(type))
                return null;

            _singletons.Add(type, @out);
            return @out;
        }*/

        public T RegisterSingleton<T>(T singleton) where T : ISingleton
        {
            var type = typeof(T);
            if (_singletons.ContainsKey(type))
                return null;

            singleton.Kernel = this;
            _singletons.Add(type, singleton);
            return singleton;
        }

        public static Kernel Create()
        {
            return new Kernel();
        }
    }
}
