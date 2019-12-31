using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Formula.Core;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;

namespace Formula.SimpleMembership
{
    public class MembershipService
    {
        protected readonly AppUserManager _userManager;
        protected readonly SignInManager<ApplicationUser> _signInManager;

        public MembershipService(
            AppUserManager userManager,
            SignInManager<ApplicationUser> signInManager)
        {
            _userManager = userManager;
            _signInManager = signInManager;
        }

        public virtual async Task<StatusBuilder> LoginAsync(LoginDetails details)
        {
            var output = new StatusBuilder();
            var results = new LoginResults();

            results.User = await _userManager.FindByNameAsync(details.Username);

            if (results.User != null) {

                var passwordValid = await _userManager.CheckPasswordAsync(results.User, details.Password);

                if (passwordValid)
                {
                    results.Result = await _signInManager.PasswordSignInAsync(details.Username, details.Password, true, lockoutOnFailure : false);
                }
                else
                {
                    // It's an invalid password, but include both username and password to lower hacking attempts
                    output.RecordFailure("Invalid Password or Username");
                }
            }
            else 
            {
                // It's an invalid username, but include both username and password to lower hacking attempts
                output.RecordFailure("Invalid Username or Password");
            }

            output.SetData(results);
            
            return output;
        }

        public async Task<StatusBuilder> RegisterAsync(RegistrationDetails details)
        {
            var output = new StatusBuilder();

            var user = await _userManager.FindByNameAsync(details.Username);

            if (user == null) 
            {
                user = new ApplicationUser {
                    Id = Guid.NewGuid().ToString(),
                    UserName = details.Username,
                    Email = details.Email
                };

                output.SetData(user);

                var identityResult = await _userManager.CreateAsync(user, details.Password);

                if (identityResult.Succeeded) 
                {
                    if (details.StartFreeTrial) 
                    {
                        var trialClaim = new Claim ("Trial", DateTime.Now.ToString ());
                        await _userManager.AddClaimAsync(user, trialClaim);
                    } 
                    else if (details.IsAdmin) 
                    {
                        await _userManager.AddToRoleAsync(user, "Admin");
                    }
                } 
                else 
                {
                    foreach(var err in identityResult.Errors)
                    {
                        output.RecordFailure(err.Description, err.Code);
                    }
                }
            }
            else
            {
                output.RecordFailure("User already exists");
            }

            return output;
        }

        public async Task<UserClaims> GetClaimsAsync(ClaimsPrincipal user) 
        {
            var loggedInUser = await _userManager.GetUserAsync(user);
            var userClaims = await _userManager.GetClaimsAsync(loggedInUser);

            var claims = userClaims
                .Union(user.Claims)
                .Select (c => new ClaimDetails {
                    Type = c.Type,
                    Value = c.Value
                });

            return new UserClaims {
                UserName = user.Identity.Name,
                Claims = claims
            };
        }

        public AuthenticationDetails GetAuthenticationDetails(ClaimsPrincipal user)
        {
            return new AuthenticationDetails {
                IsAuthenticated = user.Identity.IsAuthenticated,
                Username = user.Identity.IsAuthenticated ? user.Identity.Name : string.Empty
            };
        }

        public Task SignOutAsync()
        {
            return _signInManager.SignOutAsync();
        }

    }
}
