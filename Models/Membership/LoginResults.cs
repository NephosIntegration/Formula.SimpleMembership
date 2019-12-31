using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;

namespace Formula.SimpleMembership
{
    public class LoginResults
    {
        public ApplicationUser User { get; set; }
        public SignInResult Result { get; set; }
    }
}
