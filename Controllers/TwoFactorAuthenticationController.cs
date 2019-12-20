using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using Formula.Core;
using Formula.SimpleAPI;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using NETCore.Encrypt;
using SignInResult = Microsoft.AspNetCore.Identity.SignInResult;

namespace Formula.SimpleMembership
{
    [Route("[controller]/[action]")]
    public class TwoFactorAuthenticationController : SimpleControllerBase
    {

        private TwoFactorService _twoFactorService;

        public TwoFactorAuthenticationController(
            AppUserManager userManager,
            SignInManager<ApplicationUser> signInManager, 
            UrlEncoder urlEncoder)
        {
            _twoFactorService = new TwoFactorService(userManager, signInManager, urlEncoder);
        }

        [HttpGet]
        [Authorize]
        public Task<AccountDetails> Details()
        {
            return _twoFactorService.GetAccountDetailsAsync(User);
        }

        [HttpGet]
        [Authorize]
        public Task<TwoFactorAuthDetails> SetupAuthenticator()
        {
            return _twoFactorService.GetAuthenticatorDetailsAsync(User);
        }

        [HttpGet]
        [Authorize]
        public Task<List<int>> ValidAutheticatorCodes()
        {
            return _twoFactorService.ValidAutheticatorCodes(User);
        }

        [HttpPost]
        [Authorize]
        public async Task<StatusBuilder> VerifyAuthenticator([FromBody] VerificationCodeDetails code)
        {
            var results = this.HandleModelState();

            if (results.IsSuccessful) 
            {
                results = await _twoFactorService.VerifyAuthenticationCode(User, code);
                if (results.IsSuccessful)
                {
                    var recoveryCodes = results.GetDataAs<List<String>>();
                }
            }

            return results;
        }

        [HttpPost]
        [Authorize]
        public Task<StatusBuilder> ResetAuthenticator()
        {
            return _twoFactorService.ResetAuthenticator(User);
        }

        [HttpPost]
        [Authorize]
        public Task<StatusBuilder> Disable2FA()
        {
            return _twoFactorService.Disable2FA(User);
        }

        [HttpPost]
        [Authorize]
        public async Task<ResultVM> GenerateRecoveryCodes()
        {
            var user = await _twoFactorService.GetUserManager().GetUserAsync(User);

            var isTwoFactorEnabled = await _twoFactorService.GetUserManager().GetTwoFactorEnabledAsync(user);

            if (!isTwoFactorEnabled)
            {
                return new ResultVM
                {
                    Status = ResultStatus.Error,
                    Message = "Cannot generate recovery codes as you do not have 2FA enabled"
                };
            }

            var recoveryCodes = await _twoFactorService.GetUserManager().GenerateNewTwoFactorRecoveryCodesAsync(user, 10);

            return new ResultVM
            {
                Status = ResultStatus.Success,
                Message = "You have generated new recovery codes",
                Data = new { recoveryCodes }
            };
        }

        /// <summary>
        /// Entry point into the login workflow
        /// </summary>
        [HttpGet]
        public IActionResult Login(string returnUrl)
        {
            return View();
        }

        [HttpPost]
        //public async Task<ResultVM> Login(TwoFactorLoginVM model, string button)
        public async Task<IActionResult> Login(TwoFactorLoginDetails model, string button)
        {
            if (ModelState.IsValid)
            {
                var result = await TwoFaLogin(model.TwoFactorCode, isRecoveryCode: false, model.RememberMachine);
                if (result.Status == ResultStatus.Success) 
                {
                    return Redirect("~/");
                }
                else
                {
                    ModelState.AddModelError(string.Empty, result.Message);
                }
            }

            return View(model);

            /*
            var errors = GetErrors(ModelState).Select(e => "<li>" + e + "</li>");
            return new ResultVM
            {
                Status = ResultStatus.Error,
                Message = "Invalid data",
                Data = string.Join("", errors)
            };
            */
        }

        [HttpPost]
        public async Task<ResultVM> LoginWithRecovery([FromBody] TwoFactorRecoveryCodeLoginDetails model)
        {
            if (ModelState.IsValid)
            {
                return await TwoFaLogin(model.RecoveryCode, isRecoveryCode: true);
            }

            var errors = GetErrors(ModelState).Select(e => "<li>" + e + "</li>");
            return new ResultVM
            {
                Status = ResultStatus.Error,
                Message = "Invalid data",
                Data = string.Join("", errors)
            };
        }

        [HttpGet]
        public IActionResult AesKey()
        {
            return Ok(EncryptProvider.CreateAesKey().Key);
        }

        #region Private methods

        private async Task<ResultVM> TwoFaLogin(string code, bool isRecoveryCode, bool rememberMachine = false)
        {
            SignInResult result = null;

            var user = await _twoFactorService.GetSignInManager().GetTwoFactorAuthenticationUserAsync();

            if (user == null)
            {
                return new ResultVM
                {
                    Status = ResultStatus.Error,
                    Message = "Invalid data",
                    Data = "<li>Unable to load two-factor authentication user</li>"
                };
            }

            var authenticatorCode = code.Replace(" ", string.Empty).Replace("-", string.Empty);

            if (!isRecoveryCode)
            {
                result = await _twoFactorService.GetSignInManager().TwoFactorAuthenticatorSignInAsync(authenticatorCode, true,
                    rememberMachine);
            }
            else
            {
                result = await _twoFactorService.GetSignInManager().TwoFactorRecoveryCodeSignInAsync(authenticatorCode);
            }

            if (result.Succeeded)
            {
                return new ResultVM
                {
                    Status = ResultStatus.Success,
                    Message = $"Welcome {user.UserName}"
                };
            }
            else if (result.IsLockedOut)
            {
                return new ResultVM
                {
                    Status = ResultStatus.Error,
                    Message = "Invalid data",
                    Data = "<li>Account locked out</li>"
                };
            }
            else
            {
                return new ResultVM
                {
                    Status = ResultStatus.Error,
                    Message = "Invalid data",
                    Data = $"<li>Invalid {(isRecoveryCode ? "recovery" : "authenticator")} code</li>"
                };
            }
        }

        #endregion
    }
}
