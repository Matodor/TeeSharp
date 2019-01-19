using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using TeeSharp.Common.Enums;
using BenchmarkDotNet.Attributes;

namespace TeeSharp.Benchmark
{
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct StructSnapshotPlayerInput
    {
        public int Direction;
        public int TargetX;
        public int TargetY;
        public int Jump;
        public int Fire;
        public int Hook;
        public PlayerFlags PlayerFlags;
        public Weapon WantedWeapon;
        public Weapon NextWeapon;
        public Weapon PreviousWeapon;
    }

    public class MarshalBenchmark
    {
        private StructSnapshotPlayerInput _input;

        public MarshalBenchmark()
        {
            _input = new StructSnapshotPlayerInput()
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

        [Benchmark(Description = "sizeof")]
        public unsafe void Sizeof()
        {
            for (var i = 0; i < 100000; i++)
            {
                var size = sizeof(StructSnapshotPlayerInput);
            }
        }

        [Benchmark(Description = "Unsafe.SizeOf")]
        public void Unsafe_SizeOf()
        {
            for (var i = 0; i < 100000; i++)
            {
                var size = Unsafe.SizeOf<StructSnapshotPlayerInput>();
            }
        }

        [Benchmark(Description = "Marshal.StructureToPtr")]
        public void Marshal_StructureToPtr()
        {
            for (var i = 0; i < 100000; i++)
            {
                var bytes = Write(_input);
            }
        }
        
        private byte[] Write(in StructSnapshotPlayerInput input)
        {
            var array = new byte[Marshal.SizeOf<StructSnapshotPlayerInput>()];
            var handle = GCHandle.Alloc(array, GCHandleType.Pinned);
            var ptr = handle.AddrOfPinnedObject();

            Marshal.StructureToPtr(input, ptr, false);
            handle.Free();

            return array;
        }

        [Benchmark(Description = "MemoryMarshal.Write")]
        public void DeserializationMemoryMarshal()
        {
            for (var i = 0; i < 100000; i++)
            {
                var span = new Span<byte>(new byte[Unsafe.SizeOf<StructSnapshotPlayerInput>()]);
                MemoryMarshal.Write(span, ref _input);
            }
        }
    }
}