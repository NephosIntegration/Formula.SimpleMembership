using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Formula.SimpleMembership
{
    public class UserClaims
    {
        public IEnumerable<ClaimVM> Claims { get; set; }
        public string UserName { get; set; }
    }
}
