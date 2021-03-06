using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using Formula.Core;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.Logging;

namespace Formula.SimpleMembership
{
    public class TwoFactorService
    {
        protected readonly AppUserManager _userManager;
        protected readonly SignInManager<ApplicationUser> _signInManager;
        private readonly UrlEncoder _urlEncoder;
        private readonly IEmailSender _emailSender;
        private readonly ISmsSender _smsSender;


        /// Delete These : Begin
        public AppUserManager GetUserManager()
        {
            return _userManager;
        }

        public SignInManager<ApplicationUser> GetSignInManager()
        {
            return _signInManager;
        }
        /// Delete These : End

        public TwoFactorService(
            AppUserManager userManager,
            SignInManager<ApplicationUser> signInManager,
            UrlEncoder urlEncoder,
            IEmailSender emailSender,
            ISmsSender smsSender)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _urlEncoder = urlEncoder;
            _emailSender = emailSender;
            _smsSender = smsSender;
        }

        public async Task<AccountDetails> GetAccountDetailsAsync(ClaimsPrincipal principal)
        {
            var user = await _userManager.GetUserAsync(principal);
            var logins = await _userManager.GetLoginsAsync(user);

            return new AccountDetails
            {
                Username = user.UserName,
                Email = user.Email,
                EmailConfirmed = user.EmailConfirmed,
                PhoneNumber = user.PhoneNumber,
                ExternalLogins = logins.Select(login => login.ProviderDisplayName).ToList(),
                TwoFactorEnabled = await _userManager.GetTwoFactorEnabledAsync(user),
                HasAuthenticator = await _userManager.GetAuthenticatorKeyAsync(user) != null,
                TwoFactorClientRemembered = await _signInManager.IsTwoFactorClientRememberedAsync(user),
                RecoveryCodesLeft = await _userManager.CountRecoveryCodesAsync(user)
            };
        }

        public async Task<List<int>> ValidAutheticatorCodes(ClaimsPrincipal principal)
        {
            List<int> validCodes = new List<int>();

            var user = await _userManager.GetUserAsync(principal);

            var key = await _userManager.GetAuthenticatorKeyAsync(user);

            var hash = new HMACSHA1(Identity.Internals.Base32.FromBase32(key));
            var unixTimestamp = Convert.ToInt64(Math.Round((DateTime.UtcNow - new DateTime(1970, 1, 1, 0, 0, 0)).TotalSeconds));
            var timestep = Convert.ToInt64(unixTimestamp / 30);
            // Allow codes from 90s in each direction (we could make this configurable?)
            for (int i = -2; i <= 2; i++)
            {
                var expectedCode = Identity.Internals.Rfc6238AuthenticationService.ComputeTotp(hash, (ulong)(timestep + i), modifier: null);
                validCodes.Add(expectedCode);
            }

            return validCodes;
        }

        public Task<SignInResult> TwoFactorSignInAsync(VerifyCodeViewModel model)
        {
            return _signInManager.TwoFactorSignInAsync(model.Provider, model.Code, model.RememberMe, model.RememberBrowser);
        }

        // The data returned by the status builder upon success is any available recovery codes
        public async Task<StatusBuilder> VerifyAuthenticatorCode(ClaimsPrincipal principal, VerificationCodeDetails code)
        {
            var output = new StatusBuilder();

            var user = await _userManager.GetUserAsync(principal);

            var verificationCode = code.VerificationCode.Replace(" ", string.Empty).Replace("-", string.Empty);

            var is2FaTokenValid = await _userManager.VerifyTwoFactorTokenAsync(user, _userManager.Options.Tokens.AuthenticatorTokenProvider, verificationCode);

            if (is2FaTokenValid)
            {
                var results = await _userManager.SetTwoFactorEnabledAsync(user, true);

                if (results.Succeeded)
                {
                    output.SetMessage("Your authenticator app has been verified");

                    if (await _userManager.CountRecoveryCodesAsync(user) == 0)
                    {
                        var recoveryCodes = await _userManager.GenerateNewTwoFactorRecoveryCodesAsync(user, 10);
                        output.SetData(recoveryCodes.ToList());
                    }
                }
                else
                {
                    foreach(var err in results.Errors)
                    {
                        output.RecordFailure(err.Description, err.Code);
                    }
                }
            }
            else
            {
                output.RecordFailure("Verification code is invalid");
            }

            return output;
        }

        public async Task<StatusBuilder> ResetAuthenticator(ClaimsPrincipal principal)
        {
            var output = new StatusBuilder();

            var user = await _userManager.GetUserAsync(principal);

            await _userManager.SetTwoFactorEnabledAsync(user, false);
            await _userManager.ResetAuthenticatorKeyAsync(user);

            await _signInManager.RefreshSignInAsync(user);

            output.SetMessage("Your authenticator app key has been reset, you will need to configure your authenticator app using the new key.");

            return output;
        }

        public async Task<StatusBuilder> Disable2FA(ClaimsPrincipal principal)
        {
            var output = new StatusBuilder();

            var user = await _userManager.GetUserAsync(principal);

            if (await _userManager.GetTwoFactorEnabledAsync(user))
            {
                var result = await _userManager.SetTwoFactorEnabledAsync(user, false);
                if (result.Succeeded)
                {
                    output.SetMessage("2FA has been successfully disabled");
                }
                else
                {
                    output.RecordFailure($"Failed to disable 2FA {result.Errors.FirstOrDefault()?.Description}");
                }
            }
            else
            {
                output.RecordFailure("Cannot disable 2FA as it's not currently enabled");
            }

            return output;
        }

        public async Task<StatusBuilder> GenerateRecoveryCodes(ClaimsPrincipal principal)
        {
            var output = new StatusBuilder();

            var user = await _userManager.GetUserAsync(principal);

            var isTwoFactorEnabled = await _userManager.GetTwoFactorEnabledAsync(user);

            if (isTwoFactorEnabled)
            {
                var recoveryCodes = await _userManager.GenerateNewTwoFactorRecoveryCodesAsync(user, 10);
                
                output.SetMessage("You have generated new recovery codes")
                      .SetData(recoveryCodes.ToList());
            }
            else
            {
                output.RecordFailure("Cannot generate recovery codes as you do not have 2FA enabled");
            }

            return output;
        }

        public Task<SignInResult> TwoFaLogin(VerifyAuthenticatorCodeViewModel model)
        {
            return _signInManager.TwoFactorAuthenticatorSignInAsync(model.Code, model.RememberMe, model.RememberBrowser);
        }

        public Task<SignInResult> TwoFaRecovery(UseRecoveryCodeViewModel model)
        {
            return _signInManager.TwoFactorRecoveryCodeSignInAsync(model.Code);
        }

        public async Task<StatusBuilder> TwoFaLogin(string code, bool isRecoveryCode, bool rememberMachine = false)
        {
            var output = await Validate2faUser();

            if (output.IsSuccessful)
            {
                var user = output.GetDataAs<ApplicationUser>();
                
                var authenticatorCode = code.Replace(" ", string.Empty).Replace("-", string.Empty);

                SignInResult result = null;

                if (!isRecoveryCode)
                {
                    result = await _signInManager.TwoFactorAuthenticatorSignInAsync(authenticatorCode, true, rememberMachine);
                }
                else
                {
                    result = await _signInManager.TwoFactorRecoveryCodeSignInAsync(authenticatorCode);
                }

                if (result.Succeeded)
                {
                    output.SetMessage($"Welcome {user.UserName}");
                }
                else if (result.IsLockedOut)
                {
                    output.RecordFailure("Account locked out");
                }
                else
                {
                    output.RecordFailure($"Invalid {(isRecoveryCode ? "recovery" : "authenticator")} code");
                }
            }

            return output;
        }


        public async Task<StatusBuilder> Validate2faUser()
        {
            var output = new StatusBuilder();

            var user = await  _signInManager.GetTwoFactorAuthenticationUserAsync();
            if (user == null)
            {
                output.RecordFailure("Unable to load two-factor authentication user");
            }
            else
            {
                output.SetData(user);
            }

            return output;
        }

        public async Task<StatusBuilder> GetOptions(string returnUrl = null, bool rememberMe = false)
        {
            var output = await Validate2faUser();

            if (output.IsSuccessful)
            {
                var user = output.GetDataAs<ApplicationUser>();
                var userFactors = await _userManager.GetValidTwoFactorProvidersAsync(user);
                var factorOptions = userFactors.Select(purpose => new SelectListItem { Text = purpose, Value = purpose }).ToList();
                output.SetData(new SendCodeViewModel { Providers = factorOptions, ReturnUrl = returnUrl, RememberMe = rememberMe });
            }

            return output;
        }

        public async Task<StatusBuilder> SendToken(ApplicationUser user, SendCodeViewModel model)
        {
            var output = new StatusBuilder();

            // Generate the token and send it
            var code = await _userManager.GenerateTwoFactorTokenAsync(user, model.SelectedProvider);
            if (string.IsNullOrWhiteSpace(code))
            {
                output.RecordFailure("Unable to generate token");
            }
            else
            {
                var message = "Your security code is: " + code;

                if (model.SelectedProvider == "Email")
                {
                    await _emailSender.SendEmailAsync(await _userManager.GetEmailAsync(user), "Security Code", message);
                }
                else if (model.SelectedProvider == "Phone")
                {
                    await _smsSender.SendSmsAsync(await _userManager.GetPhoneNumberAsync(user), message);
                }            
            }

            return output;
        }





        public async Task<TwoFactorAuthDetails> GetAuthenticatorDetailsAsync(ClaimsPrincipal principal, String issuer)
        {
            var user = await _userManager.GetUserAsync(principal);
            var authenticatorDetails = await GetAuthenticatorDetailsAsync(user, issuer);
            return authenticatorDetails;
        }

        public async Task<TwoFactorAuthDetails> GetAuthenticatorDetailsAsync(ApplicationUser user, String issuer)
        {
            // Load the authenticator key & QR code URI to display on the form
            var unformattedKey = await _userManager.GetAuthenticatorKeyAsync(user);
            if (string.IsNullOrEmpty(unformattedKey))
            {
                await _userManager.ResetAuthenticatorKeyAsync(user);
                unformattedKey = await _userManager.GetAuthenticatorKeyAsync(user);
            }

            var email = await _userManager.GetEmailAsync(user);

            return new TwoFactorAuthDetails
            {
                SharedKey = FormatKey(unformattedKey),
                AuthenticatorUri = GenerateQrCodeUri(email, issuer, unformattedKey)
            };
        }

        private string FormatKey(string unformattedKey)
        {
            var result = new StringBuilder();
            int currentPosition = 0;
            while (currentPosition + 4 < unformattedKey.Length)
            {
                result.Append(unformattedKey.Substring(currentPosition, 4)).Append(" ");
                currentPosition += 4;
            }
            if (currentPosition < unformattedKey.Length)
            {
                result.Append(unformattedKey.Substring(currentPosition));
            }

            return result.ToString();
        }

        private string GenerateQrCodeUri(string email, string issuer, string unformattedKey)
        {
            const string AuthenticatorUriFormat = "otpauth://totp/{0}:{1}?secret={2}&issuer={0}&digits=6";

            return string.Format(
                AuthenticatorUriFormat,
                _urlEncoder.Encode(issuer),
                _urlEncoder.Encode(email),
                unformattedKey);
        }

    }
}
