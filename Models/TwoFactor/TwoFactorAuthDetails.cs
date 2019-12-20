using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Formula.SimpleMembership
{
    public class TwoFactorAuthDetails
    {
        public string SharedKey { get; set; }

        public string AuthenticatorUri { get; set; }
    }
}
