using System;
using System.Linq.Expressions;
using System.Reflection;

namespace TeeSharp.Common
{
    public abstract class Binder
    {
        public Type BindedType { get; }
        public Type InjectedType { get; protected set; } = null;
        public Func<BaseInterface> Activator { get; protected set; }

        protected BaseInterface Singleton { get; set; }

        protected Binder(Type bindedType)
        {
            BindedType = bindedType;
        }
    }

    public class Binder<TBinded> : Binder where TBinded : BaseInterface
    {
        public Binder() : base(typeof(TBinded))
        {
            Activator = () => throw new Exception($"Type '{typeof(TBinded).Name}' not binded");
        }

        private void CheckInjectedType()
        {
            if (InjectedType == null)
                throw new NullReferenceException($"");
            if (!InjectedType.IsClass)
                throw new Exception($"Injected type ({InjectedType.Name}) must be a class with public constructor without parameters");
        }

        public void AsSingleton()
        {
            CheckInjectedType();
            Singleton = Activator();
            Activator = () => Singleton;
        }

        public Binder<TBinded> To<TInjected>() where TInjected : BaseInterface, new()
        {
            InjectedType = typeof(TInjected);
            CheckInjectedType();
            Activator = BindedActivator<TInjected>.Activator;
            return this;
        }

        public Binder<TBinded> ToSelf()
        {
            InjectedType = typeof(TBinded);
            CheckInjectedType();
            Activator = BindedActivator<TBinded>.Activator;
            return this;
        }
    }

    public static class BindedActivator<T> where T : BaseInterface
    {
        public static readonly Func<BaseInterface> Activator;

        static BindedActivator()
        {
            var constructor = typeof(T).GetConstructor(BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Public, null,
                new Type[0], null);
            var e = Expression.New(constructor);
            Activator = Expression.Lambda<Func<BaseInterface>>(e).Compile();
        }
    }
}