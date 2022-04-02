using BenchmarkDotNet.Attributes;

namespace TeeSharp.Benchmark;

public class VirtualCallBenchmark
{
    private class GameModeA
    {
        public void TestMethod()
        {
            for (var i = 0; i < 1000000; i++)
            {
                var x = 1 + 2;
            }
        }
    }
        
    private abstract class GameModeBase
    {
        public abstract void TestMethod();
    }
        
    private class GameModeB : GameModeBase
    {
        public override void TestMethod()
        {
            for (var i = 0; i < 1000000; i++)
            {
                var x = 1 + 2;
            }
        }
    }
        
    private class GameModeC : GameModeB
    {
        public override void TestMethod()
        {
            for (var i = 0; i < 1000000; i++)
            {
                var x = 1 + 2;
            }
        }
    }
        
    private interface IGameMode
    {
        void TestMethod();
    }

    private class GameModeD : IGameMode
    {
        public virtual void TestMethod()
        {
            for (var i = 0; i < 1000000; i++)
            {
                var x = 1 + 2;
            }
        }
    }

    private class GameModeE : GameModeD
    {
        public override void TestMethod()
        {
            for (var i = 0; i < 1000000; i++)
            {
                var x = 1 + 2;
            }
        }
    }
        
    [Benchmark(Description = "Test1")]
    public void Test1()
    {
        var gameMode = new GameModeA();
        gameMode.TestMethod();
    }
        
    [Benchmark(Description = "Test2")]
    public void Test2()
    {
        var gameMode = new GameModeB();
        gameMode.TestMethod();
    }
        
    [Benchmark(Description = "Test3")]
    public void Test3()
    {
        var gameMode = new GameModeC();
        gameMode.TestMethod();
    }
        
    [Benchmark(Description = "Test4")]
    public void Test4()
    {
        var gameMode = new GameModeD();
        gameMode.TestMethod();
    }    
        
    [Benchmark(Description = "Test5")]
    public void Test5()
    {
        var gameMode = new GameModeE();
        gameMode.TestMethod();
    }
}