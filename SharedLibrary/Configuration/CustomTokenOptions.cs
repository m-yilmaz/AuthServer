﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SharedLibrary.Configuration
{
    public class CustomTokenOptions
    {
        public List<String> Audience { get; set; }
        public string Issuer { get; set; }
        public int AccessTokenExpiration { get; set; }
        public int RefleshTokenExpiration { get; set; }
        public string SecurityKey { get; set; }

    }
}