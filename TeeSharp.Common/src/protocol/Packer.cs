using System;
using System.Text;
using TeeSharp.Core;

namespace TeeSharp.Common
{
    public class Packer
    {
        private const int PackerBufferSize = 1024 * 2;

        public bool Error { get; private set; }

        private readonly byte[] _buffer;
        private int _index;

        public Packer()
        {
            _buffer = new byte[PackerBufferSize];
            Reset();
        }

        public void Reset()
        {
            Error = false;
            _index = 0;
        }

        public byte[] Data()
        {
            return _buffer;
        }

        public int Size()
        {
            return _index;
        }


        public void AddInt(int value)
        {
            if (Error)
                return;

            if (_index + 4 >= PackerBufferSize)
                Error = true;
            else
                _index = IntCompression.Pack(_buffer, _index, value);
        }

        public void AddString(string value, int limit = 0)
        {
            if (value == null || Error)
                return;

            var strBytes = Encoding.UTF8.GetBytes(value.Limit(limit));
            if (strBytes.Length > 0)
            {
                if (_index + strBytes.Length >= PackerBufferSize)
                {
                    Error = true;
                    return;
                }

                Buffer.BlockCopy(strBytes, 0, _buffer, _index, strBytes.Length);
                _index += strBytes.Length;
            }
            _buffer[_index++] = 0;
        }

        public void AddRaw(byte[] inputData)
        {
            AddRaw(inputData, 0, inputData.Length);
        }

        public void AddRaw(byte[] inputData, int inputOffset, int inputSize)
        {
            if (inputSize <= 0 || inputOffset < 0 || Error)
                return;

            if (_index + inputSize >= PackerBufferSize)
            {
                Error = true;
                return;
            }

            Buffer.BlockCopy(inputData, inputOffset, _buffer, _index, inputSize);
            _index += inputSize;
        }
    }
}