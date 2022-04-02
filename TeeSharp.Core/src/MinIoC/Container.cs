// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.
// https://github.com/microsoft/MinIoC/blob/master/Container.cs

using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace TeeSharp.Core.MinIoC;

public partial class Container : Container.IScope
{
    private readonly Dictionary<Type, Func<ILifetime, object>> _registeredTypes =
        new Dictionary<Type, Func<ILifetime, object>>();

    private readonly ContainerLifetime _lifetime;
        
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
        if (typeof(IContainerService).IsAssignableFrom(itemType))
        {
            var sourceFactory = factory;
            factory = lifetime =>
            {
                var obj = sourceFactory(lifetime);
                if (obj is IContainerService service)
                    service.Container = this;

                return obj;
            };
        }

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