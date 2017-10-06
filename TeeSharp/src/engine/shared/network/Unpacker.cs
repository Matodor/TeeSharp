using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TeeSharp
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

            int i;
            _currentIndex = IntCompression.Unpack(_buffer, _currentIndex, out i);
            if (_currentIndex >= _endIndex)
            {
                Error = true;
                return 0;
            }

            return i;
        }

        public string GetString(SanitizeType sanitizeType = 0)
        {
            if (Error || _currentIndex >= _endIndex)
                return "";

            byte b;
            List<byte> bytes = new List<byte>();

            while ((b = _buffer[_currentIndex]) != 0) // skip the string
            {
                bytes.Add(b);
                ++_currentIndex;

                if (_currentIndex >= _endIndex)
                {
                    Error = true;
                    return "";
                }
            }
            var strUTF8 = Encoding.UTF8.GetString(bytes.ToArray());
            ++_currentIndex;

            if ((sanitizeType & SanitizeType.SANITIZE) != 0)
                strUTF8 = strUTF8.Sanitize();
            else if ((sanitizeType & SanitizeType.SANITIZE_CC) != 0)
                strUTF8 = strUTF8.SanitizeCC();

            return (sanitizeType & SanitizeType.SKIP_START_WHITESPACES) != 0 
                ? strUTF8.TrimStart(new[] { ' ', '\t', '\n', '\r' }) 
                : strUTF8;
        }
    }
}
