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
        public Task<StatusBuilder> GenerateRecoveryCodes()
        {
            return _twoFactorService.GenerateRecoveryCodes(User);
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
        public async Task<StatusBuilder> Login(TwoFactorLoginDetails model, string button)
        {
            var results = this.HandleModelState();

            if (results.IsSuccessful) 
            {
                results = await _twoFactorService.TwoFaLogin(model.TwoFactorCode, isRecoveryCode: false, model.RememberMachine);
            }

            return results;
        }

        [HttpPost]
        public async Task<StatusBuilder> LoginWithRecovery(TwoFactorRecoveryCodeLoginDetails model)
        {
            var results = this.HandleModelState();

            if (results.IsSuccessful) 
            {
                results = await _twoFactorService.TwoFaLogin(model.RecoveryCode, isRecoveryCode: true);
            }

            return results;
        }

        [HttpGet]
        public IActionResult AesKey()
        {
            return Ok(EncryptProvider.CreateAesKey().Key);
        }
    }
}
