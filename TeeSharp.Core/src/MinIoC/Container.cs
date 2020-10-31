// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.
// https://github.com/microsoft/MinIoC/blob/master/Container.cs

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace TeeSharp.Core.MinIoC
{
    public class Container : Container.IScope
    {
        private readonly Dictionary<Type, Func<ILifetime, object>> _registeredTypes =
            new Dictionary<Type, Func<ILifetime, object>>();

        private readonly ContainerLifetime _lifetime;
        
        public interface IScope : IDisposable, IServiceProvider
        {
        }

        public interface IRegisteredType
        {
            void AsSingleton();
            void PerScope();
        }

        private interface ILifetime : IScope
        {
            object GetServiceAsSingletone(Type type, Func<ILifetime, object> factory);
            object GetServicePerScope(Type type, Func<ILifetime, object> factory);
        }

        private abstract class ObjectCache
        {
            private readonly ConcurrentDictionary<Type, object> _instanceCache =
                new ConcurrentDictionary<Type, object>();

            protected object GetCached(Type type, Func<ILifetime, object> factory, ILifetime lifetime)
            {
                return _instanceCache.GetOrAdd(type, _ => factory(lifetime));
            }

            public void Dispose()
            {
                foreach (var obj in _instanceCache.Values)
                {
                    (obj as IDisposable)?.Dispose();
                }
            }
        }

        private class ContainerLifetime : ObjectCache, ILifetime
        {
            public Func<Type, Func<ILifetime, object>> GetFactory { get; private set; }

            public ContainerLifetime(Func<Type, Func<ILifetime, object>> getFactory)
            {
                GetFactory = getFactory;
            }

            public object GetService(Type type)
            {
                return GetFactory(type)(this);
            }

            public object GetServiceAsSingletone(Type type, Func<ILifetime, object> factory)
            {
                return GetCached(type, factory, this);
            }

            public object GetServicePerScope(Type type, Func<ILifetime, object> factory)
            {
                return GetServiceAsSingletone(type, factory);
            }
        }

        private class ScopeLifetime : ObjectCache, ILifetime
        {
            private readonly ContainerLifetime _parentLifetime;

            public ScopeLifetime(ContainerLifetime parent)
            {
                _parentLifetime = parent;
            }

            public object GetService(Type type)
            {
                return _parentLifetime.GetFactory(type)(this);
            }

            public object GetServiceAsSingletone(Type type, Func<ILifetime, object> factory)
            {
                return _parentLifetime.GetServiceAsSingletone(type, factory);
            }

            public object GetServicePerScope(Type type, Func<ILifetime, object> factory)
            {
                return GetCached(type, factory, this);
            }
        }

        private class RegisteredType : IRegisteredType
        {
            private readonly Type _itemType;
            private readonly Action<Func<ILifetime, object>> _registerFactory;
            private readonly Func<ILifetime, object> _factory;

            public RegisteredType(
                Type itemType, 
                Action<Func<ILifetime, object>> registerFactory,
                Func<ILifetime, object> factory)
            {
                _itemType = itemType;
                _registerFactory = registerFactory;
                _factory = factory;

                registerFactory(_factory);
            }

            public void AsSingleton()
            {
                _registerFactory(lifetime => lifetime.GetServiceAsSingletone(_itemType, _factory));
            }

            public void PerScope()
            {
                _registerFactory(lifetime => lifetime.GetServicePerScope(_itemType, _factory));
            }
        }


        public Container()
        {
            _lifetime = new ContainerLifetime(t => _registeredTypes[t]);
        }

        public IRegisteredType Register(Type @interface, Func<object> factory)
        {
            return RegisterType(@interface, _ => factory());
        }

        public IRegisteredType Register(Type @interface, Type implementation)
        {
            return RegisterType(@interface, FactoryFromType(implementation));
        }

        private IRegisteredType RegisterType(Type itemType, Func<ILifetime, object> factory)
        {
            return new RegisteredType(itemType, f => _registeredTypes[itemType] = f, factory);
        }

        public object GetService(Type type)
        {
            return _registeredTypes.TryGetValue(type, out var registeredType)
                ? registeredType(_lifetime)
                : null;
        }

        public IScope CreateScope()
        {
            return new ScopeLifetime(_lifetime);
        }

        public void Dispose()
        {
            _lifetime.Dispose();
        }

        private static Func<ILifetime, object> FactoryFromType(Type itemType)
        {
            var constructors = itemType.GetConstructors();
            if (constructors.Length == 0)
                constructors = itemType.GetConstructors(BindingFlags.Instance | BindingFlags.NonPublic);

            var constructor = constructors.First();
            var arg = Expression.Parameter(typeof(ILifetime));

            return (Func<ILifetime, object>) Expression.Lambda(
                Expression.New(constructor, constructor.GetParameters().Select(
                    param =>
                    {
                        var resolve = new Func<ILifetime, object>(
                            lifetime => lifetime.GetService(param.ParameterType));
                        return Expression.Convert(
                            Expression.Call(Expression.Constant(resolve.Target), resolve.Method, arg),
                            param.ParameterType);
                    })
                ),
                arg
            ).Compile();
        }
    }
}
