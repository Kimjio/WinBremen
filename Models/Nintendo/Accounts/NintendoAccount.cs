using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WinBremen.Models.Nintendo.Accounts
{
    class NintendoAccount
    {
        public bool analyticsOptedIn { get; set; }

        public long analyticsOptedInUpdatedAt { get; set; }

        public AnalyticsPermissions analyticsPermissions { get; set; }

        public string birthday { get; set; }

        public Mii[] candidateMiis { get; set; }

        public bool clientFriendsOptedIn { get; set; }

        public long clientFriendsOptedInUpdatedAt { get; set; }

        public string country { get; set; }

        public long createdAt { get; set; }

        public EachEmailOptedIn eachEmailOptedIn { get; set; }

        public bool emailOptedIn { get; set; }

        public long emailOptedInUpdatedAt { get; set; }

        public bool emailVerified { get; set; }

        public string gender { get; set; }

        public string iconUrl { get; set; }

        public string id { get; set; }

        public bool isChild { get; set; }

        public string language { get; set; }

        public Mii mii { get; set; }

        public string nickname { get; set; }

        public bool phoneNumberEnabled { get; set; }

        public string region { get; set; }

        public string screenName { get; set; }

        public TimeZone timezone { get; set; }

        public long updatedAt { get; set; }

        internal class AnalyticsPermissions
        {
            public Permissions internalAnalysis { get; set; }

            public Permissions targetMarketing { get; set; }

            internal class Permissions
            {
                public bool permitted { get; set; }

                public long updatedAt { get; set; }
            }
        }

        internal class EachEmailOptedIn
        {
            public OptedIn deals { get; set; }
            public OptedIn survey { get; set; }

            internal class OptedIn
            {
                public bool optedIn { get; set; }

                public long updatedAt { get; set; }
            }
        }

        internal class TimeZone
        {
            public string id { get; set; }

            public string name { get; set; }

            public string utcOffset { get; set; }

            public int utcSeconds { get; set; }
        }

    }
}
