using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace WinBremen.Utils
{
    class HttpUtils
    {
        public static readonly HttpClient client = new(new HttpClientHandler()
            { AutomaticDecompression = System.Net.DecompressionMethods.GZip }
        );
    }
}
