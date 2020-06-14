using System.Net;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TeeSharp.Core.IoC;
using TeeSharp.Network;
using TeeSharp.Network.Extensions;

namespace TeeSharp.Tests
{
    [TestClass]
    public class ContainerTests
    {
        private class TestBaseClass
        {
        }

        private class TestClass : TestBaseClass
        {
        }

        [TestMethod]
        public void TestContainerSingletone()
        {
            var container = new Container();
            container.Register<TestBaseClass>(typeof(TestClass)).AsSingleton();
            
            Assert.AreSame(
                container.Resolve<TestBaseClass>(), 
                container.Resolve<TestBaseClass>() 
            );
        }

        [TestMethod]
        public void TestContainerScope()
        {
            var container = new Container();
            container.Register<TestBaseClass>(typeof(TestClass)).PerScope();

            var instance1 = container.Resolve<TestBaseClass>();
            var instance2 = container.Resolve<TestBaseClass>();

            Assert.AreEqual(instance1, instance2);

            using (var scope = container.CreateScope())
            {
                var instance3 = scope.Resolve<TestBaseClass>();
                var instance4 = scope.Resolve<TestBaseClass>();

                Assert.AreEqual(instance3, instance4);
                Assert.AreNotEqual(instance1, instance3);
            }
        }
    }
}