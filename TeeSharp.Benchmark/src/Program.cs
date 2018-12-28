using System;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Running;
using TeeSharp.Common.Enums;
using TeeSharp.Common.Protocol;

namespace TeeSharp.Benchmark
{
    public class EquatableSnapshotPlayerInput
    {
        private SnapshotPlayerInput _first;
        private SnapshotPlayerInput _second;

        public EquatableSnapshotPlayerInput()
        {
            _first = new SnapshotPlayerInput()
            {
                Direction = -1,
                PlayerFlags = PlayerFlags.Admin | PlayerFlags.Bot | PlayerFlags.Dead,
                WantedWeapon = Weapon.Grenade,
                Hook = 1,
                Jump = 1,
                TargetY = 123,
                PreviousWeapon = Weapon.Ninja,
                Fire = 555,
                NextWeapon = Weapon.Hammer,
                TargetX = 999,
            };

            _second = new SnapshotPlayerInput()
            {
                Direction = -1,
                PlayerFlags = PlayerFlags.Admin | PlayerFlags.Bot | PlayerFlags.Dead,
                WantedWeapon = Weapon.Grenade,
                Hook = 1,
                Jump = 1,
                TargetY = 123,
                PreviousWeapon = Weapon.Ninja,
                Fire = 555,
                NextWeapon = Weapon.Hammer,
                TargetX = 999,
            };
        }

        [Benchmark]
        public void MethodEquals()
        {
            for (var i = 0; i < 100000; i++)
            {
                var equals = _first.Equals(_second);
            }
        }

        [Benchmark]
        public void MethodMarshal()
        {
            for (var i = 0; i < 100000; i++)
            {
                var data1 = _first.ToArray();
                var data2 = _second.ToArray();
                var equals = true;

                for (var index = 0; index < data1.Length; index++)
                {
                    if (data1[index] != data2[index])
                    {
                        equals = false;
                        break;
                    }
                }
            }
        }
    }

    internal class Program
    {
        static void Main(string[] args)
        {
            var summary = BenchmarkRunner.Run<EquatableSnapshotPlayerInput>();
            Console.ReadLine();
        }
    }
}
