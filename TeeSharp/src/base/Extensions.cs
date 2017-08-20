using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TeeSharp
{
    public static class Extensions
    {
        public static string Limit(this string source, int maxLength)
        {
            if (maxLength <= 0 || source.Length <= maxLength)
                return source;
            return source.Substring(0, maxLength);
        }

        /* makes sure that the string only contains the characters between 32 and 255 */
        public static string SanitizeCC(this string str)
        {
            var tmp = new StringBuilder(str);
            for (int i = 0; i < tmp.Length; i++)
            {
                if (tmp[i] < 32)
                    tmp[i] = ' ';
            }

            return tmp.ToString();
        }

        /* makes sure that the string only contains the characters between 32 and 255 + \r\n\t */
        public static string Sanitize(this string str)
        {
            var tmp = new StringBuilder(str);
            for (int i = 0; i < tmp.Length; i++)
            {
                if (tmp[i] < 32 && tmp[i] != '\r' && tmp[i] != '\n' && tmp[i] != '\t')
                    tmp[i] = ' ';
            }

            return tmp.ToString();
        }
    }
}
