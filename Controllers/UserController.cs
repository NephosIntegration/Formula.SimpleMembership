using System;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace Formula.SimpleMembership
{
    [Route("[controller]/[action]")]
    public class UserController : Controller {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;

        public UserController (UserManager<ApplicationUser> userManager, SignInManager<ApplicationUser> signInManager) {
            _userManager = userManager;
            _signInManager = signInManager;
        }

        [HttpPost]
        public async Task<ResultVM> Register ([FromBody] RegisterVM model) {
            if (ModelState.IsValid) {
                IdentityResult result = null;
                var user = await _userManager.FindByNameAsync (model.UserName);

                if (user != null) {
                    return new ResultVM {
                    Status = ResultStatus.Error,
                    Message = "Invalid data",
                    Data = "User already exists"
                    };
                }

                user = new ApplicationUser {
                    Id = Guid.NewGuid ().ToString (),
                    UserName = model.UserName,
                    Email = model.Email
                };

                result = await _userManager.CreateAsync (user, model.Password);

                if (result.Succeeded) {
                    if (model.StartFreeTrial) {
                        Claim trialClaim = new Claim ("Trial", DateTime.Now.ToString ());
                        await _userManager.AddClaimAsync (user, trialClaim);
                    } else if (model.IsAdmin) {
                        await _userManager.AddToRoleAsync (user, "Admin");
                    }

                    return new ResultVM {
                        Status = ResultStatus.Success,
                            Message = "User Created",
                            Data = user
                    };
                } else {
                    var resultErrors = result.Errors.Select (e => "" + e.Description + "");
                    return new ResultVM {
                        Status = ResultStatus.Error,
                            Message = "Invalid data",
                            Data = string.Join ("", resultErrors)
                    };
                }
            }

            var errors = ModelState.Keys.Select (e => "" + e + "");
            return new ResultVM {
                Status = ResultStatus.Error,
                    Message = "Invalid data",
                    Data = string.Join ("", errors)
            };
        }

        [HttpPost]
        public async Task<ResultVM> Login ([FromBody] LoginVM model) {
            if (ModelState.IsValid) {
                var user = await _userManager.FindByNameAsync(model.UserName);

                if (user != null && await _userManager.CheckPasswordAsync(user, model.Password)) {

                    await _signInManager.PasswordSignInAsync(model.UserName, model.Password, true, lockoutOnFailure : false);

                    return new ResultVM {
                        Status = ResultStatus.Success,
                            Message = "Succesfull login",
                            Data = model
                    };
                }

                return new ResultVM {
                    Status = ResultStatus.Error,
                        Message = "Invalid data",
                        Data = "Invalid Username or Password"
                };
            }

            var errors = ModelState.Keys.Select (e => "" + e + "");
            return new ResultVM {
                Status = ResultStatus.Error,
                    Message = "Invalid data",
                    Data = string.Join ("", errors)
            };
        }

        [HttpGet]
        [Authorize]
        public async Task<UserClaims> Claims () {
            var loggedInUser = await _userManager.GetUserAsync (User);
            var userClaims = await _userManager.GetClaimsAsync(loggedInUser);

            var claims = userClaims.Union(User.Claims).Select (c => new ClaimVM {
                Type = c.Type,
                    Value = c.Value
            });

            return new UserClaims {
                UserName = User.Identity.Name,
                    Claims = claims
            };
        }

        [HttpGet]
        public async Task<UserStateVM> Authenticated () {
            return new UserStateVM {
                IsAuthenticated = User.Identity.IsAuthenticated,
                    Username = User.Identity.IsAuthenticated ? User.Identity.Name : string.Empty
            };
        }

        [HttpPost]
        public async Task SignOut () {
            await _signInManager.SignOutAsync ();
        }
    }
}
