using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WinBremen.Models.Nintendo.Accounts;

namespace WinBremen.Models.Nintendo.BaaS
{
    class BasSUser
    {
        public string id { get; set; }

        public string nickname { get; set; }

        public string country { get; set; }

        public string birthday { get; set; }

        public string gender { get; set; }

        public DeviceAccount[] deviceAccounts { get; set; }

        public Links links { get; set; }

        public Permissions permissions { get; set; }

        public long createdAt { get; set; }

        public long updatedAt { get; set; }

        public bool hasUnreadCsComment { get; set; }

        internal class Links
        {
            public LinkedAccount nintendoAccount { get; set; }

            internal class LinkedAccount
            {
                public string id { get; set; }

                public long createdAt { get; set; }

                public long updatedAt { get; set; }

                public NintendoAccount userinfo { get; set; }
            }
        }

        internal class Permissions
        {
            public bool personalAnalytics { get; set; }
            public bool personalNotification { get; set; }
            public long personalAnalyticsUpdatedAt { get; set; }
            public long personalNotificationUpdatedAt { get; set; }
        }
    }
}
