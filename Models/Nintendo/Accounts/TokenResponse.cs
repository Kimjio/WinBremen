using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WinBremen.Models.Nintendo.Accounts
{
    class TokenResponse
    {
        public string accessToken { get; set; }

        public string idToken { get; set; }

        public Dictionary<string, object> error { get; set; }

        public int expiresIn { get; set; }

        public object termsAgreement { get; set; }

        public NintendoAccount user { get; set; }
    }
}
