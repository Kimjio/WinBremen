using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WinBremen.Models.Nintendo.BaaS
{
    class LoginRequestWithDeviceAccount : LoginRequest
    {
        public DeviceAccount deviceAccount { get; set; }
    }
}
