using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WinBremen.Models.Nintendo.BaaS
{
    internal class LoginResponse
    {

        public string idToken { get; set; }

        public string accessToken { get; set; }

        public User user { get; set; }

        public DeviceAccount createdDeviceAccount { get; set; }

        public string sessionId { get; set; }

        public ErrorWrapper error { get; set; }

        public int expiresIn { get; set; }

        public string market { get; set; }

        public object capability { get; set; }

        public object behaviorSettings { get; set; }

        internal class User
        {
            public string id { get; set; }

            public string nickname { get; set; }

            public string country { get; set; }

            public string birthday { get; set; }

            public string gender { get; set; }

            public DeviceAccount[] deviceAccounts { get; set; }

            public object links { get; set; }

            public Permissions permissions { get; set; }

            public long createdAt { get; set; }

            public long updatedAt { get; set; }

            public bool hasUnreadCsComment { get; set; }

            internal class Permissions
            {
                public bool personalAnalytics { get; set; }
                public bool personalNotification { get; set; }
                public long personalAnalyticsUpdatedAt { get; set; }
                public long personalNotificationUpdatedAt { get; set; }
            }
        }

        internal class DeviceAccount
        {
            public string id { get; set; }
            public string password { get; set; }
        }

        internal class ErrorWrapper
        {
            public int errorCode { get; set; }

            public Error errorMessage { get; set; }
        }
    }
}
