using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WinBremen.Utils
{
    internal class RandomUtils
    {
        /// <summary>
        /// ['a'...'z', '0'...'9', 'A'...'Z']
        /// </summary>
        private static readonly char[] chars;
        private static readonly Random random = new();

        static RandomUtils()
        {
            List<char> list = [];

            char c = 'a';
            while (c <= 'z')
            {
                list.Add(c);
                c += '\x1';
            }

            c = '0';
            while (c <= '9')
            {
                list.Add(c);
                c += '\x1';
            }

            c = 'A';
            while (c <= 'Z')
            {
                list.Add(c);
                c += '\x1';
            }

            chars = [.. list];
        }

        private RandomUtils() { }

        public static string GenerateRandomString(int size)
        {
            string tmp = string.Empty;

            for (int i = 0; i < size; i++)
            {
                tmp += chars[random.Next(chars.Length - 1)];
            }

            return tmp;
        }
    }
}
