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
            var tmp = new StringBuilder(str.Length);
            for (var i = 0; i < str.Length; i++)
            {
                if (str[i] < 32)
                    continue;
                tmp.Append(str[i]);
            }

            return tmp.ToString();
        }

        /* makes sure that the string only contains the characters between 32 and 255 + \r\n\t */
        public static string Sanitize(this string str)
        {
            var tmp = new StringBuilder(str.Length);
            for (var i = 0; i < str.Length; i++)
            {
                if (str[i] < 32 && str[i] != '\r' && str[i] != '\n' && str[i] != '\t')
                    continue;
                tmp.Append(str[i]);
            }

            return tmp.ToString();
        }
    }
}
