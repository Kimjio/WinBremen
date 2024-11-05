using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace WinBremen.Models.Nintendo.BaaS
{
    internal class LoginRequest
    {
        public string appVersion { get; set; } = "";

        public string assertion { get; set; }

        public string deviceName { get; set; } = "";

        public string locale { get; set; } = "";

        public string manufacturer { get; set; } = "";

        [JsonConverter(typeof(JsonStringEnumConverter))]
        public NetworkType networkType { get; set; } = NetworkType.unknown;

        [JsonConverter(typeof(JsonStringEnumConverter))]
        public OSType osType { get; set; }

        public string osVersion { get; set; } = "";

        public string sessionId { get; set; }

        public string sdkVersion { get; set; } = "";

        public string timeZone { get; set; } = "";

        public long timeZoneOffset { get; set; } = 0;
        
        public enum NetworkType
        { 
            wifi,
            wwan,
            unknown,
        }
        
        public enum OSType
        {
            Android,
            iOS
        }
    }
}
