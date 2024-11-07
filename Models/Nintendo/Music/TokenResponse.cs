using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WinBremen.Models.Nintendo.Music
{
    class TokenResponse
    {
        public string edgeToken { get; set; }

        public string token { get; set; }

        public NXBaasUserInfo user { get; set; }
    }
}
