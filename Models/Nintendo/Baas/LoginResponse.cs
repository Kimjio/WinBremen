using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WinBremen.Models.Nintendo.BaaS
{
    class LoginResponse
    {

        public string idToken { get; set; }

        public string accessToken { get; set; }

        public BasSUser user { get; set; }

        public DeviceAccount createdDeviceAccount { get; set; }

        public string sessionId { get; set; }

        public ErrorWrapper error { get; set; }

        public int? expiresIn { get; set; }

        public string market { get; set; }

        public Dictionary<string, object> capability { get; set; }

        public Dictionary<string, object> behaviorSettings { get; set; }

        internal class ErrorWrapper
        {
            public int errorCode { get; set; }

            public Error errorMessage { get; set; }
        }
    }
}
