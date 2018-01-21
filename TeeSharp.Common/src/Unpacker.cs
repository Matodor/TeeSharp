using System;
using System.Collections.Generic;
using System.Text;

namespace TeeSharp.Common
{
    [Flags]
    public enum SanitizeType
    {
        SANITIZE = 1,
        SANITIZE_CC = 2,
        SKIP_START_WHITESPACES = 4
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
            if (_currentIndex >= _endIndex)
            {
                Error = true;
                return 0;
            }

            return result;
        }

        public string GetString(SanitizeType sanitizeType = 0)
        {
            if (Error || _currentIndex >= _endIndex)
                return "";

            var startIndex = _currentIndex;
            while (_buffer[_currentIndex] == 0)
            {
                _currentIndex++;
                if (_currentIndex >= _endIndex)
                {
                    Error = true;
                    return string.Empty;
                }
            }

            var strUTF8 = Encoding.UTF8.GetString(
                _buffer, startIndex, _currentIndex - startIndex);
            _currentIndex++;

            if ((sanitizeType & SanitizeType.SANITIZE) != 0)
                strUTF8 = strUTF8.Sanitize();
            else if ((sanitizeType & SanitizeType.SANITIZE_CC) != 0)
                strUTF8 = strUTF8.SanitizeCC();

            return (sanitizeType & SanitizeType.SKIP_START_WHITESPACES) != 0
                ? strUTF8.TrimStart(' ', '\t', '\n', '\r')
                : strUTF8;
        }
    }
}