using System;
using System.Linq;
using System.Threading.Tasks;
using Formula.Core;
using Formula.SimpleAPI;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace Formula.SimpleMembership
{
    [Route("[controller]/[action]")]
    public class UserController : SimpleControllerBase 
    {
        private MembershipAccountService _accountService;

        public UserController (AppUserManager userManager, SignInManager<ApplicationUser> signInManager) {
            _accountService = new MembershipAccountService(userManager, signInManager);
        }

        [HttpPost]
        public async Task<StatusBuilder> Login ([FromBody] LoginDetails model) 
        {
            var results = this.HandleModelState();

            if (results.IsSuccessful) 
            {
                results = await _accountService.LoginAsync(model);
            }

            return results;
        }

        [HttpPost]
        public async Task<StatusBuilder> Register ([FromBody] RegistrationDetails model) 
        {
            var results = this.HandleModelState();

            if (results.IsSuccessful) 
            {
                results = await _accountService.RegisterAsync(model);
            }

            return results;
        }

        [HttpGet]
        [Authorize]
        public Task<UserClaims> Claims() 
        {
            return _accountService.GetClaimsAsync(User);
        }

        [HttpGet]
        public AuthenticationDetails Authenticated()
        {
            return _accountService.GetAuthenticationDetails(User);
        }

        [HttpPost]
        public Task SignOut() 
        {
            return _accountService.SignOutAsync();
        }
    }
}
