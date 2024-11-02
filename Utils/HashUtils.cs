using System;
using System.Globalization;
using System.Linq;
using System.Numerics;
using System.Security.Cryptography;
using System.Text;

namespace WinBremen.Utils
{
    internal class HashUtils
    {
        private HashUtils() {}

        public static string GetHash(string isu, int size)
        {
            var key = Encoding.UTF8.GetBytes(isu);

            var t1 = (DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() / 1000);
            var time = (t1 / 600).ToString("X").ToUpper();

            time = time.PadLeft(16, '0');

            var data = BigInteger.Parse($"10{time}", NumberStyles.HexNumber).ToByteArray(false, true);
            var length = data.Length - 1;

            byte[] buffer = new byte[length];

            Array.Copy(data, 1, buffer, 0, length);

            var hmac = new HMACSHA1(key);
            hmac.Initialize();
            var computed = hmac.ComputeHash(buffer).Select(x => (sbyte)x).ToArray();   

            var i = computed[^1] & 15;
            var num = (((computed[i + 3] & 255) | ((((computed[i] & SByte.MaxValue) << 24) | ((computed[i + 1] & 255) << 16)) | ((computed[i + 2] & 255) << 8))) % ((int)MathF.Pow(10, size))).ToString();
            num = num.PadLeft(size, '0');

            return num;
        }
    }
}
