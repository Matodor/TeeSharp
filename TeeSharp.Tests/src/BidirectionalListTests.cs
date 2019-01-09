using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TeeSharp.Core;

namespace TeeSharp.Tests
{
    [TestClass]
    public class BidirectionalListTests
    {
        [TestMethod]
        public void TestInsert()
        {
            var list = BidirectionalList<int>.New();
            var node1 = list.Add(1);
            var node2 = list.Add(2);
            var node3 = list.Add(3);
            var node4 = list.Add(4);
            var node5 = list.Add(5);

            foreach (var i in list) Console.Write($"{i} "); Console.Write('\n');
            list.RemoveFast(node5);
            foreach (var i in list) Console.Write($"{i} "); Console.Write('\n');
            list.RemoveFast(node4);
            foreach (var i in list) Console.Write($"{i} "); Console.Write('\n');
            list.RemoveFast(node3);
            foreach (var i in list) Console.Write($"{i} "); Console.Write('\n');
            list.RemoveFast(node2);
            foreach (var i in list) Console.Write($"{i} "); Console.Write('\n');
            list.RemoveFast(node1);
            foreach (var i in list) Console.Write($"{i} "); Console.Write('\n');

            Assert.AreSame(null, list.First);
            Assert.AreSame(null, list.Last);
        }
    }
}