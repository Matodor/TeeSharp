using System;
using System.Text;
using TeeSharp.Core;

namespace TeeSharp.Common
{
    public class Packer
    {
        public const int MAX_PACKER_BUFFER_SIZE = 1024 * 2;

        public bool Error { get; private set; }

        private readonly byte[] _buffer;
        private int _currentIndex;

        public Packer()
        {
            _buffer = new byte[MAX_PACKER_BUFFER_SIZE];
            _currentIndex = 0;
        }

        public byte[] Data()
        {
            return _buffer;
        }

        public int Size()
        {
            return _currentIndex;
        }

        public void Reset()
        {
            Error = false;
            _currentIndex = 0;
        }

        public void AddInt(int value)
        {
            if (Error)
                return;

            if (_currentIndex + 4 >= MAX_PACKER_BUFFER_SIZE)
                Error = true;
            else
                _currentIndex = IntCompression.Pack(_buffer, _currentIndex, value);
        }

        public void AddString(string value, int limit = 0)
        {
            if (value == null || Error)
                return;

            var strBytes = Encoding.UTF8.GetBytes(value.Limit(limit));
            if (strBytes.Length > 0)
            {
                if (_currentIndex + strBytes.Length >= MAX_PACKER_BUFFER_SIZE)
                {
                    Error = true;
                    return;
                }

                Buffer.BlockCopy(strBytes, 0, _buffer, _currentIndex, strBytes.Length);
                _currentIndex += strBytes.Length;
            }
            _buffer[_currentIndex++] = 0;
        }

        public void AddRaw(byte[] inputData)
        {
            AddRaw(inputData, 0, inputData.Length);
        }

        public void AddRaw(byte[] inputData, int inputOffset, int inputSize)
        {
            if (inputSize <= 0 || inputOffset < 0 || Error)
                return;

            if (_currentIndex + inputSize >= MAX_PACKER_BUFFER_SIZE)
            {
                Error = true;
                return;
            }

            Buffer.BlockCopy(inputData, inputOffset, _buffer, _currentIndex, inputSize);
            _currentIndex += inputSize;
        }
    }
}