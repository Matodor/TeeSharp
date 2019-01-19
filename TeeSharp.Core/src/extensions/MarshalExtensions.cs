using System;
using System.IO;
using System.Runtime.InteropServices;

namespace TeeSharp.Core.Extensions
{
    public static class MarshalExtensions
    {
        // TODO MemoryMarshal and Unsafe class
        // https://youtu.be/Do0mcw8hwYo
        // MemoryMarshal.Cast<>()

        private static Array Read(this Span<byte> buffer, Type elementType, int elementSize, int reads = 1)
        {
            var array = Array.CreateInstance(elementType, reads);
            var handle = GCHandle.Alloc(buffer.ToArray(), GCHandleType.Pinned);

            for (var i = 0; i < array.Length; i++)
                array.SetValue(Marshal.PtrToStructure(handle.AddrOfPinnedObject() + elementSize * i, elementType), i);
            
            handle.Free();
            return array;
        }

        public static T ReadArray<T>(this Span<byte> buffer)
        {
            var type = typeof(T);
            if (!type.IsArray)
                throw new Exception($"MarshalExtensions.{nameof(ReadArray)} for read structs use Read method");

            var elementType = type.GetElementType();
            var elementSize = Marshal.SizeOf(elementType);
            return (T) (object) Read(buffer, elementType, elementSize, buffer.Length / elementSize);
        }

        public static T ReadArray<T>(this Stream stream, int arrayElements)
        {
            var type = typeof(T);
            if (!type.IsArray)
                throw new Exception($"MarshalExtensions.{nameof(ReadArray)} for read structs use Read method");

            var elementType = type.GetElementType();
            var elementSize = Marshal.SizeOf(elementType);
            var buffer = (Span<byte>) new byte[elementSize * arrayElements];
            stream.Read(buffer);
            return (T) (object) Read(buffer, elementType, elementSize, arrayElements);
        }

        public static T Read<T>(this Span<byte> buffer)
        {
            var type = typeof(T);
            if (type.IsArray)
                throw new Exception($"MarshalExtensions.{nameof(Read)} for array of structs use ReadArray method");

            var elementSize = Marshal.SizeOf(type);
            return (T) Read(buffer, type, elementSize).GetValue(0);
        }

        public static T Read<T>(this Stream stream)
        {
            var type = typeof(T);
            if (type.IsArray)
                throw new Exception($"MarshalExtensions.{nameof(Read)} for array of structs use ReadArray method");

            var elementSize = Marshal.SizeOf(type);
            var buffer = (Span<byte>) new byte[elementSize];
            stream.Read(buffer);
            return (T) Read(buffer, type, elementSize).GetValue(0);
        }
    }
}