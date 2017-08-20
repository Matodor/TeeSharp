using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TeeSharp
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
            if (string.IsNullOrEmpty(value) || Error)
                return;

            var strBytes = Encoding.UTF8.GetBytes(value.Limit(limit));
            if (_currentIndex + strBytes.Length >= MAX_PACKER_BUFFER_SIZE)
            {
                Error = true;
                return;
            }

            Array.Copy(strBytes, 0, _buffer, _currentIndex, strBytes.Length);
            _currentIndex += strBytes.Length;
            _buffer[_currentIndex++] = 0;
        }

        public void AddRaw(byte[] data, int dataIndex, int size)
        {
            if (size <= 0 || dataIndex < 0 || Error)
                return;

            if (_currentIndex + size >= MAX_PACKER_BUFFER_SIZE)
            {
                Error = true;
                return;
            }

            Array.Copy(data, dataIndex, _buffer, _currentIndex, size);
            _currentIndex += size;
        }
    }
}
