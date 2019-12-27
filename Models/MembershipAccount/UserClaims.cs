using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Formula.SimpleMembership
{
    public class UserClaims
    {
        public IEnumerable<ClaimDetails> Claims { get; set; }
        public string UserName { get; set; }
    }
}
