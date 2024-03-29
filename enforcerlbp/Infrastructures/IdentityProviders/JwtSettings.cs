﻿using System;
using System.Collections.Generic;
using System.Text;

namespace Infrastructures.IdentityProviders
{
    public class JwtSettings
    {
        public string Secret { get; set; }
        public string Issuer { get; set; }
        public string Audience { get; set; }
        public string ExpirationTime { get; set; }
    }
}
