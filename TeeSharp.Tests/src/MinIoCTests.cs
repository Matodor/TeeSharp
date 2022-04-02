// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.
// https://github.com/microsoft/MinIoC/blob/master/Tests/ContainerTests.cs

using System;
using NUnit.Framework;
using TeeSharp.Core.MinIoC;
using System.Collections.Generic;

namespace TeeSharp.Tests;

public class MinIoCTests
{
    private Container Container { get; set; }
        
    [SetUp]
    public void SetUp()
    {
        Container = new Container();
    }
        
    [Test]
    public void SimpleReflectionConstruction()
    {
        Container.Register<IFoo>(typeof(Foo));

        object instance = Container.Resolve<IFoo>();

        // Instance should be of the registered type 
        Assert.IsInstanceOf<Foo>(instance);
    }

    [Test]
    public void RecursiveReflectionConstruction()
    {
        Container.Register<IFoo>(typeof(Foo));
        Container.Register<IBar>(typeof(Bar));
        Container.Register<IBaz>(typeof(Baz));

        var instance = Container.Resolve<IBaz>();

        // Test that the correct types were created
        Assert.IsInstanceOf<Baz>(instance);

        var baz = instance as Baz;
        Assert.IsInstanceOf<Bar>(baz.Bar);
        Assert.IsInstanceOf<Foo>(baz.Foo);
    }

    [Test]
    public void SimpleFactoryConstruction()
    {
        Container.Register<IFoo>(() => new Foo());

        object instance = Container.Resolve<IFoo>();

        // Instance should be of the registered type 
        Assert.IsInstanceOf<Foo>(instance);
    }

    [Test]
    public void MixedConstruction()
    {
        Container.Register<IFoo>(() => new Foo());
        Container.Register<IBar>(typeof(Bar));
        Container.Register<IBaz>(typeof(Baz));

        var instance = Container.Resolve<IBaz>();

        // Test that the correct types were created
        Assert.IsInstanceOf<Baz>(instance);

        var baz = instance as Baz;
        Assert.IsInstanceOf<Bar>(baz.Bar);
        Assert.IsInstanceOf<Foo>(baz.Foo);
    }

    [Test]
    public void InstanceResolution()
    {
        Container.Register<IFoo>(typeof(Foo));

        object instance1 = Container.Resolve<IFoo>();
        object instance2 = Container.Resolve<IFoo>();

        // Instances should be different between calls to Resolve
        Assert.AreNotEqual(instance1, instance2);
    }

    [Test]
    public void SingletonResolution()
    {
        Container.Register<IFoo>(typeof(Foo)).AsSingleton();

        object instance1 = Container.Resolve<IFoo>();
        object instance2 = Container.Resolve<IFoo>();

        // Instances should be identic between calls to Resolve
        Assert.AreEqual(instance1, instance2);
    }

    [Test]
    public void PerScopeResolution()
    {
        Container.Register<IFoo>(typeof(Foo)).PerScope();

        object instance1 = Container.Resolve<IFoo>();
        object instance2 = Container.Resolve<IFoo>();

        // Instances should be same as the container is itself a scope
        Assert.AreEqual(instance1, instance2);

        using (var scope = Container.CreateScope())
        {
            object instance3 = scope.Resolve<IFoo>();
            object instance4 = scope.Resolve<IFoo>();

            // Instances should be equal inside a scope
            Assert.AreEqual(instance3, instance4);

            // Instances should not be equal between scopes
            Assert.AreNotEqual(instance1, instance3);
        }
    }

    [Test]
    public void MixedScopeResolution()
    {
        Container.Register<IFoo>(typeof(Foo)).PerScope();
        Container.Register<IBar>(typeof(Bar)).AsSingleton();
        Container.Register<IBaz>(typeof(Baz));

        using (var scope = Container.CreateScope())
        {
            Baz instance1 = scope.Resolve<IBaz>() as Baz;
            Baz instance2 = scope.Resolve<IBaz>() as Baz;

            // Ensure resolutions worked as expected
            Assert.AreNotEqual(instance1, instance2);

            // Singleton should be same
            Assert.AreEqual(instance1.Bar, instance2.Bar);
            Assert.AreEqual((instance1.Bar as Bar).Foo, (instance2.Bar as Bar).Foo);

            // Scoped types should be the same
            Assert.AreEqual(instance1.Foo, instance2.Foo);

            // Singleton should not hold scoped object
            Assert.AreNotEqual(instance1.Foo, (instance1.Bar as Bar).Foo);
            Assert.AreNotEqual(instance2.Foo, (instance2.Bar as Bar).Foo);
        }
    }

    [Test]
    public void SingletonScopedResolution()
    {
        Container.Register<IFoo>(typeof(Foo)).AsSingleton();
        Container.Register<IBar>(typeof(Bar)).PerScope();

        var instance1 = Container.Resolve<IBar>();

        using (var scope = Container.CreateScope())
        {
            var instance2 = Container.Resolve<IBar>();

            // Singleton should resolve to the same instance
            Assert.AreEqual((instance1 as Bar).Foo, (instance2 as Bar).Foo);
        }
    }

    [Test]
    public void MixedNoScopeResolution()
    {
        Container.Register<IFoo>(typeof(Foo)).PerScope();
        Container.Register<IBar>(typeof(Bar)).AsSingleton();
        Container.Register<IBaz>(typeof(Baz));

        Baz instance1 = Container.Resolve<IBaz>() as Baz;
        Baz instance2 = Container.Resolve<IBaz>() as Baz;

        // Ensure resolutions worked as expected
        Assert.AreNotEqual(instance1, instance2);

        // Singleton should be same
        Assert.AreEqual(instance1.Bar, instance2.Bar);

        // Scoped types should not be different outside a scope
        Assert.AreEqual(instance1.Foo, instance2.Foo);
        Assert.AreEqual(instance1.Foo, (instance1.Bar as Bar).Foo);
        Assert.AreEqual(instance2.Foo, (instance2.Bar as Bar).Foo);
    }

    [Test]
    public void MixedReversedRegistration()
    {
        Container.Register<IBaz>(typeof(Baz));
        Container.Register<IBar>(typeof(Bar));
        Container.Register<IFoo>(() => new Foo());

        IBaz instance = Container.Resolve<IBaz>();

        // Test that the correct types were created
        Assert.IsInstanceOf<Baz>(instance);

        var baz = instance as Baz;
        Assert.IsInstanceOf<Bar>(baz.Bar);
        Assert.IsInstanceOf<Foo>(baz.Foo);
    }

    [Test]
    public void ScopeDisposesOfCachedInstances()
    {
        Container.Register<SpyDisposable>(typeof(SpyDisposable)).PerScope();
        SpyDisposable spy;

        using (var scope = Container.CreateScope())
        {
            spy = scope.Resolve<SpyDisposable>();
        }

        Assert.IsTrue(spy.Disposed);
    }

    [Test]
    public void ContainerDisposesOfSingletons()
    {
        SpyDisposable spy;
        using (var container = new Container())
        {
            container.Register<SpyDisposable>().AsSingleton();
            spy = container.Resolve<SpyDisposable>();
        }

        Assert.IsTrue(spy.Disposed);
    }

    [Test]
    public void SingletonsAreDifferentAcrossContainers()
    {
        var container1 = new Container();
        container1.Register<IFoo>(typeof(Foo)).AsSingleton();

        var container2 = new Container();
        container2.Register<IFoo>(typeof(Foo)).AsSingleton();

        Assert.AreNotEqual(container1.Resolve<IFoo>(), container2.Resolve<IFoo>());
    }

    [Test]
    public void GetServiceUnregisteredTypeReturnsNull()
    {
        using (var container = new Container())
        {
            object value = container.GetService(typeof(Foo));

            Assert.IsNull(value);
        }
    }

    [Test]
    public void GetServiceMissingDependencyThrows()
    {
        using (var container = new Container())
        {
            container.Register<Bar>();
            Assert.Throws<KeyNotFoundException>(() => container.GetService(typeof(Bar)));
        }
    }

    private interface IFoo
    {
    }

    private class Foo : IFoo
    {
    }

    private interface IBar
    {
    }

    private class Bar : IBar
    {
        public IFoo Foo { get; set; }

        public Bar(IFoo foo)
        {
            Foo = foo;
        }
    }

    private interface IBaz
    {
    }

    private class Baz : IBaz
    {
        public IFoo Foo { get; set; }
        public IBar Bar { get; set; }

        public Baz(IFoo foo, IBar bar)
        {
            Foo = foo;
            Bar = bar;
        }
    }

    private class SpyDisposable : IDisposable
    {
        public bool Disposed { get; private set; }

        public void Dispose() => Disposed = true;
    }
}