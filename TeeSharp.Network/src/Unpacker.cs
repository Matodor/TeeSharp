using System;
using System.Text;
using TeeSharp.Core;
using TeeSharp.Core.Extensions;

namespace TeeSharp.Network
{
    public class UnPacker
    {
        public bool Error { get; private set; }

        private byte[] _buffer;
        private int _index;
        private int _endIndex;

        public void Reset(byte[] data, int size, int offset = 0)
        {
            _buffer = data;
            _endIndex = size;
            _index = offset;

            Error = false;
        }

        public void GetBool(bool[] array)
        {
            for (var i = 0; i < array.Length; i++)
                array[i] = GetBool();
        }

        public bool GetBool()
        {
            return GetInt() == 1;
        }

        public void GetInt(int[] array)
        {
            for (var i = 0; i < array.Length; i++)
                array[i] = GetInt();
        }

        public int GetInt()
        {
            if (Error)
                return 0;

            if (_index >= _endIndex)
            {
                Error = true;
                return 0;
            }

            _index = IntCompression.Unpack(_buffer, _index, out var result);
            if (_index > _endIndex)
            {
                Error = true;
                return 0;
            }

            return result;
        }

        public void GetString(string[] array, SanitizeType flags = 0)
        {
            for (var i = 0; i < array.Length; i++)
                array[i] = GetString(flags);
        }

        public string GetString(SanitizeType flags = 0)
        {
            if (Error || _index >= _endIndex)
                return string.Empty;

            var startIndex = _index;
            while (_buffer[_index] != 0)
            {
                _index++;
                if (_index > _endIndex)
                {
                    Error = true;
                    return string.Empty;
                }
            }

            var strUTF8 = Encoding.UTF8.GetString(
                _buffer, startIndex, _index - startIndex);
            _index++;

            if (flags.HasFlag(SanitizeType.Sanitize))
                strUTF8 = strUTF8.Sanitize();
            else if (flags.HasFlag(SanitizeType.SanitizeCC))
                strUTF8 = strUTF8.SanitizeCC();

            return flags.HasFlag(SanitizeType.SkipStartWhitespaces)
                ? strUTF8.SkipWhitespaces()
                : strUTF8;
        }

        public byte[] GetRaw(int size)
        {
            if (Error)
                return null;

            if (size < 0 || _index + size > _endIndex)
            {
                Error = true;
                return null;
            }

            var data = new byte[size];
            Buffer.BlockCopy(_buffer, _index, data, 0, size);
            _index += size;
            return data;
        }
    }
}