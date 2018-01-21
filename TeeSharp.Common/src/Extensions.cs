using System.Text;

namespace TeeSharp.Common
{
    public static class Extensions
    {
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

        public static string Sanitize(this string input)
        {
            var tmp = new StringBuilder(input.Length);
            for (var i = 0; i < input.Length; i++)
            {
                if (input[i] < 32 ||
                    input[i] != '\r' ||
                    input[i] != '\n' ||
                    input[i] != '\t')
                {
                    continue;
                }

                tmp.Append(input[i]);
            }

            return tmp.ToString();
        }
    }
}