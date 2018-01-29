using System;
using System.Text;
using TeeSharp.Core;

namespace TeeSharp.Common
{
    [Flags]
    public enum SanitizeType
    {
        SANITIZE = 1 << 0,
        SANITIZE_CC = 1 << 1,
        SKIP_START_WHITESPACES = 1 << 2
    }

    public class Unpacker
    {
        public bool Error { get; private set; }

        private byte[] _buffer;
        private int _currentIndex;
        private int _endIndex;

        public void Reset(byte[] data, int size)
        {
            _buffer = data;
            _endIndex = size;
            _currentIndex = 0;

            Error = false;
        }

        public int GetInt()
        {
            if (Error)
                return 0;

            if (_currentIndex >= _endIndex)
            {
                Error = true;
                return 0;
            }

            _currentIndex = IntCompression.Unpack(_buffer, _currentIndex, out var result);
            if (_currentIndex > _endIndex)
            {
                Error = true;
                return 0;
            }

            return result;
        }

        public string GetString(SanitizeType flags = 0)
        {
            if (Error || _currentIndex >= _endIndex)
                return "";

            var startIndex = _currentIndex;
            while (_buffer[_currentIndex] != 0)
            {
                _currentIndex++;
                if (_currentIndex > _endIndex)
                {
                    Error = true;
                    return string.Empty;
                }
            }

            var strUTF8 = Encoding.UTF8.GetString(
                _buffer, startIndex, _currentIndex - startIndex);
            _currentIndex++;

            if (flags.HasFlag(SanitizeType.SANITIZE))
                strUTF8 = strUTF8.Sanitize();
            else if (flags.HasFlag(SanitizeType.SANITIZE_CC))
                strUTF8 = strUTF8.SanitizeCC();

            return flags.HasFlag(SanitizeType.SKIP_START_WHITESPACES)
                ? strUTF8.TrimStart(' ', '\t', '\n', '\r')
                : strUTF8;
        }
    }
}