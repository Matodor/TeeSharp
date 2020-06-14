using System;
using System.Linq.Expressions;
using System.Reflection;

namespace TeeSharp.Core
{
    public abstract class Binder
    {
        public Type BindedType { get; }
        public Type InjectedType { get; protected set; }
        public Func<object> Activator { get; protected set; }

        protected object Singleton { get; set; }

        protected Binder(Type bindedType)
        {
            Singleton = null;
            BindedType = bindedType;
        }
    }

    public class Binder<TBinded> : Binder
    {
        public Binder() : base(typeof(TBinded))
        {
            Activator = () => throw new Exception($"Type '{typeof(TBinded).Name}' not binded");
        }

        private void CheckInjectedType()
        {
            if (InjectedType == null)
                throw new NullReferenceException("Bind type first");
            if (!InjectedType.IsClass)
                throw new Exception($"Injected type ({InjectedType.Name}) must be a class with public constructor without parameters");
        }

        public void AsSingleton()
        {
            CheckInjectedType();
            var activator = Activator;

            Activator = () =>
            {
                if (Singleton == null)
                {
                    Singleton = activator();
                    Activator = () => Singleton;
                }

                return Singleton;
            };
        }

        public Binder<TBinded> To<TInjected>() where TInjected : TBinded, new()
        {
            InjectedType = typeof(TInjected);
            CheckInjectedType();
            Activator = BindedActivator<TInjected>.Activator;
            return this;
        }
    }

    public static class BindedActivator<T> where T : new()
    {
        public static readonly Func<object> Activator;

        static BindedActivator()
        {
            var constructor = typeof(T).GetConstructor(BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Public, null,
                new Type[0], null);
            var e = Expression.New(constructor);
            Activator = Expression.Lambda<Func<object>>(e).Compile();
        }
    }
}