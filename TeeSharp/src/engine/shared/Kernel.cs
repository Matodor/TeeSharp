using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;

namespace TeeSharp
{
    public static class Kernel
    {
        private static readonly Dictionary<Type, object> _singletons;
        private static readonly Dictionary<Type, Func<object>> _getInstancesByType;
        private static readonly Dictionary<Type, Type> _bindedTypes;
        
        static Kernel()
        {
            _bindedTypes = new Dictionary<Type, Type>();
            _singletons = new Dictionary<Type, object>();
            _getInstancesByType = new Dictionary<Type, Func<object>>();
        }

        private static void CreateInstanceActivator(Type bind, Type to)
        {
            if (_getInstancesByType.ContainsKey(bind))
                return;

            var constructor = to.GetConstructor(new Type[] { });

            if (constructor == null)
                throw new Exception("The given type has not public construcor");

            var e = Expression.New(constructor);
            var f = Expression.Lambda<Func<object>>(e).Compile();
            _getInstancesByType.Add(bind, f);
        }

        public static T Get<T>()
        {
            return (T) Get(typeof(T));
        }

        public static object Get(Type type)
        {
            if (_singletons.ContainsKey(type))
                return _singletons[type];

            if (_getInstancesByType.ContainsKey(type))
                return _getInstancesByType[type]();

            return null;
        }

        public static void Bind<T1, T2>(T2 singleton) where T2 : T1
        {
            Bind(typeof(T1), singleton);
        }

        public static void Bind(Type bind, object singleton)
        {
            if (_singletons.ContainsKey(bind))
                throw new Exception("Already binded");
            _singletons.Add(bind, singleton);
        }

        public static void Bind<T1, T2>() where T2 : T1
        {
            Bind(typeof(T1), typeof(T2));
        }
        
        public static void Bind(Type bind, Type to)
        {
            if (!bind.IsAssignableFrom(to))
                throw new Exception("");

            if (_bindedTypes.ContainsKey(bind))
                throw new Exception($"Type '{bind.Name}' already binded");

            CreateInstanceActivator(bind, to);
            _bindedTypes.Add(bind, to);
        }

        public static T1 BindGet<T1, T2>(T2 singleton) where T2 : T1
        {
            Bind(typeof(T1), singleton);
            return Get<T1>();
        }

        public static object BindGet(Type bind, object singleton)
        {
            Bind(bind, singleton);
            return Get(bind);
        }

        public static object BindGet(Type bind, Type to)
        {
            Bind(bind, to);
            return Get(bind);
        }

        public static T1 BindGet<T1, T2>() where T2 : T1
        {
            Bind(typeof(T1), typeof(T2));
            return Get<T1>();
        }
    }
}
