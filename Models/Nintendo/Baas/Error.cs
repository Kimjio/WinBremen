﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WinBremen.Models.Nintendo.BaaS
{
    internal class Error
    {
        public string type { get; set; }

        public int status { get; set; }

        public string errorCode { get; set; }

        public string title { get; set; }

        public string detail { get; set; }

        public string instance { get; set; }
    }
}
