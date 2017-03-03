using System;
using System.Collections.Generic;
using System.Text;

namespace Teecsharp
{
    public class CPacker
    {
        const int PACKER_BUFFER_SIZE = 1024 * 2;

        private readonly byte[] m_aBuffer;
        private int m_pCurrentIndex;
        private int m_pEndIndex;
        private int m_Error;

        public CPacker()
        {
            m_aBuffer = new byte[PACKER_BUFFER_SIZE];
            m_pCurrentIndex = 0;
            m_pEndIndex = m_pCurrentIndex + PACKER_BUFFER_SIZE;
        }

        public int Size()
        {
            return m_pCurrentIndex;
        }

        public byte[] Data()
        {
            return m_aBuffer;
        }

        public bool Error()
        {
            return m_Error != 0;
        }

        public void Reset()
        {
            m_Error = 0;
            m_pCurrentIndex = 0;
            m_pEndIndex = m_pCurrentIndex + PACKER_BUFFER_SIZE;
        }

        public void AddInt(int i)
        {
            if (m_Error != 0)
                return;

            // make sure that we have space enough
            if (m_pEndIndex - m_pCurrentIndex < 6)
            {
                m_Error = 1;
            }
            else
                m_pCurrentIndex = CVariableInt.Pack(m_aBuffer, m_pCurrentIndex, i);
        }

        public void AddString(string pStr, int Limit = 0)
        {
            if (string.IsNullOrEmpty(pStr) || m_Error != 0)
                return;

            //
            if (Limit > 0)
            {
                var strBytes = Encoding.UTF8.GetBytes(pStr.LimitLength(Limit));
                if (m_pCurrentIndex + strBytes.Length >= m_pEndIndex)
                {
                    m_Error = 1;
                    return;
                }
                Array.Copy(strBytes, 0, m_aBuffer, m_pCurrentIndex, strBytes.Length);
                m_pCurrentIndex += strBytes.Length;
                m_aBuffer[m_pCurrentIndex++] = 0;
            }
            else
            {
                var strBytes = Encoding.UTF8.GetBytes(pStr);
                if (m_pCurrentIndex + strBytes.Length >= m_pEndIndex)
                {
                    m_Error = 1;
                    return;
                }
                Array.Copy(strBytes, 0, m_aBuffer, m_pCurrentIndex, strBytes.Length);
                m_pCurrentIndex += strBytes.Length;
                m_aBuffer[m_pCurrentIndex++] = 0;
            }
        }

        public void AddRaw(byte[] pData, int pDataIndex, int Size)
        {
            if (Size <= 0 || pDataIndex < 0 || m_Error != 0)
                return;

            if (m_pCurrentIndex + Size >= m_pEndIndex)
            {
                m_Error = 1;
                return;
            }

            Array.Copy(pData, pDataIndex, m_aBuffer, m_pCurrentIndex, Size);
            m_pCurrentIndex += Size;
        }
    }

    public class CUnpacker
    {
        private byte[] m_aBuffer;
        private int m_pCurrentIndex;
        private int m_pEndIndex;
        private int m_Error;

        public const int
            SANITIZE = 1,
            SANITIZE_CC = 2,
            SKIP_START_WHITESPACES = 4;

        public void Reset(byte[] pData, int Size)
        {
            m_aBuffer = pData;
            m_Error = 0;
            m_pEndIndex = Size;
            m_pCurrentIndex = 0;
        }

        public int GetInt()
        {
            if (m_Error != 0)
                return 0;

            if (m_pCurrentIndex >= m_pEndIndex)
            {
                m_Error = 1;
                return 0;
            }

            int i;
            m_pCurrentIndex = CVariableInt.Unpack(m_aBuffer, m_pCurrentIndex, out i);
            if (m_pCurrentIndex > m_pEndIndex)
            {
                m_Error = 1;
                return 0;
            }
            return i;
        }

        public string GetString(int SanitizeType = 0)
        {
            if (m_Error != 0 || m_pCurrentIndex >= m_pEndIndex)
                return "";

            byte b;
            List<byte> bytes = new List<byte>();

            while ((b = m_aBuffer[m_pCurrentIndex]) != 0) // skip the string
            {
                bytes.Add(b);
                m_pCurrentIndex += 1;

                if (m_pCurrentIndex == m_pEndIndex)
                {
                    m_Error = 1;
                    return "";
                }
            }
            var strUTF8 = Encoding.UTF8.GetString(bytes.ToArray());
            m_pCurrentIndex += 1;
            
            // sanitize all strings
            if ((SanitizeType & SANITIZE) != 0)
                CSystem.str_sanitize(ref strUTF8);
            else if ((SanitizeType & SANITIZE_CC) != 0)
                CSystem.str_sanitize_cc(ref strUTF8);
            return (SanitizeType & SKIP_START_WHITESPACES) != 0 ? CSystem.str_skip_whitespaces(strUTF8) : strUTF8;
        }

        public bool Error()
        {
            return m_Error != 0;
        }
    };
}
