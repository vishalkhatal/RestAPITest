using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;
using System.Web.Http.ModelBinding;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.Owin.Security;
using Microsoft.Owin.Security.Cookies;
using Microsoft.Owin.Security.OAuth;
using RealEstate.Models;
using RealEstate.Providers;
using RealEstate.Results;
using RealEstate.DataAccesss;
using System.Net;
using System.Linq;
using RealEstate.Common;

namespace RealEstate.Controllers
{
    [Authorize]
    public class AccountController : ApiController
    {
        #region Authorization
        private const string LocalLoginProvider = "Local";
        private ApplicationUserManager _userManager;
        RealEstateContext db = new RealEstateContext();
        public AccountController()
        {
        }

        public AccountController(ApplicationUserManager userManager,
            ISecureDataFormat<AuthenticationTicket> accessTokenFormat)
        {
            UserManager = userManager;
            AccessTokenFormat = accessTokenFormat;
        }

        public ApplicationUserManager UserManager
        {
            get
            {
                return _userManager ?? Request.GetOwinContext().GetUserManager<ApplicationUserManager>();
            }
            private set
            {
                _userManager = value;
            }
        }

        public ISecureDataFormat<AuthenticationTicket> AccessTokenFormat { get; private set; }
        #endregion

        #region Used methods
               
        // POST api/Account/ChangePassword
        [HttpPost]
        public async Task<IHttpActionResult> ChangePassword(ChangePasswordBindingModel model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            IdentityResult result = await UserManager.ChangePasswordAsync(User.Identity.GetUserId(), model.OldPassword,
                model.NewPassword);

            if (!result.Succeeded)
            {
                return GetErrorResult(result);
            }

            return Ok();
        }
        
        // POST: /Account/ResetPassword
        [HttpPost]
        [AllowAnonymous]
        public async Task<IHttpActionResult> ResetPassword(ResetPasswordViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest();
            }
            try
            {
                var user = await UserManager.FindByNameAsync(model.Email);
                if (user == null)
                {
                    // Don't reveal that the user does not exist
                    return BadRequest();
                }
                var code = model.Code.Replace(" ", "+");
                var result = await UserManager.ResetPasswordAsync(user.Id, code, model.Password);
                if (result.Succeeded)
                {
                    return Ok();
                }
                return Ok();
            }
            catch(Exception e)
            {
                Logger.AddLog(e.InnerException.Message, "Reset Password method");
                return BadRequest();
            }
        }

        // POST api/Account/Register
        [HttpPost]
        [AllowAnonymous]
        public async Task<IHttpActionResult> Register(RegisterBindingModel model)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var user = new ApplicationUser()
                {
                    UserName = model.Email,
                    Email = model.Email,
                };

                IdentityResult result = await UserManager.CreateAsync(user, model.Password);
                // For more information on how to enable account confirmation and password reset please visit http://go.microsoft.com/fwlink/?LinkID=320771
                // Send an email with this link
                var userDetail = UserManager.FindByName(user.UserName);
                UserManager.AddToRole(userDetail.Id, model.UserType.ToString());

                string code = await UserManager.GenerateEmailConfirmationTokenAsync(userDetail.Id);

                //var callbackUrl = System.Configuration.ConfigurationManager.AppSettings["AppBaseAddress"] + "Account/ConfirmEmail?userId=" + userDetail.Id + "&code=" + code;

                //await UserManager.SendEmailAsync(user.Id,
                //   "Confirm your account",
                //   "Please confirm your account by clicking: <a href=\""
                //                                   + callbackUrl + "\">link</a>");

                var userData = new User();
                userData.FirstName = model.FirstName;
                userData.LastName = model.LastName;
                userData.MobileNo = model.MobNo;
                userData.UserId = userDetail.Id;
                userData.UserName = userDetail.UserName;
                userData.Email = userDetail.UserName;
                userData.CreatedAt = DateTime.Today;
                userData.UserType = model.UserType;
                db.Users.Add(userData);
                db.SaveChanges();
                if (!result.Succeeded)
                {
                    return GetErrorResult(result);
                }

                return Ok();
            }
            catch (Exception e)
            {
                Logger.AddLog(e.InnerException.Message, "Register method");
                return BadRequest();

            }
        }

        [HttpPost]
        [AllowAnonymous]
        public async Task<IHttpActionResult> ConfirmEmail(ConfirmAccountViewModel model)
        {
            if (model.userId == null || model.Code == null)
            {
                return BadRequest();
            }
            try
            {
                var code = model.Code.Replace(" ", "+");
                var result = await UserManager.ConfirmEmailAsync(model.userId, code);
                if (result.Succeeded)
                {
                    return Ok();
                }
                else
                    return BadRequest();
            }
            catch(Exception e)
            {
                Logger.AddLog(e.InnerException.Message, "ConfirmEmail method");
                return BadRequest();
            }
        }

        // POST api/Account/Login
        [HttpPost]
        [AllowAnonymous]
        public HttpResponseMessage Login(LoginViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return Request.CreateResponse(HttpStatusCode.BadRequest, false);
            }

            try
            {
                var userDetail = UserManager.FindByName(model.UserName);
                //if (userDetail != null && userDetail.EmailConfirmed)
                //{
                if (UserManager.CheckPassword(userDetail, model.Password))
                {
                    var userInfo = db.Users.Where(x => x.UserId == userDetail.Id).FirstOrDefault();
                    return Request.CreateResponse(HttpStatusCode.OK, userInfo);
                }
                //}
                return Request.CreateResponse(HttpStatusCode.Unauthorized);
            }
            catch (Exception e)
            {
                Logger.AddLog(e.InnerException.Message, "Login method");
                return Request.CreateResponse(HttpStatusCode.Unauthorized);
            }
        }

        [HttpPost]
        [AllowAnonymous]
        public async Task<IHttpActionResult> ForgotPassword(ForgotPasswordViewModel model)
        {
            if (ModelState.IsValid)
            {
                var user = await UserManager.FindByNameAsync(model.Email);
                // If user has to activate his email to confirm his account, the use code listing below
                //if (user == null || !(await UserManager.IsEmailConfirmedAsync(user.Id)))
                //{
                //    return Ok();
                //}
                if (user == null)
                {
                    return Ok();
                }

                // For more information on how to enable account confirmation and password reset please visit http://go.microsoft.com/fwlink/?LinkID=320771
                // Send an email with this link
                string code = await UserManager.GeneratePasswordResetTokenAsync(user.Id);
                try
                {

                    var callbackUrl = System.Configuration.ConfigurationManager.AppSettings["AppBaseAddress"] + "Account/ResetPassword?userId=" + user.Id + "&code=" + code;
                    await UserManager.SendEmailAsync(user.Id,
                       "Reset your password",
                       "Please reset your password by clicking this link: <a href=\""
                                                       + callbackUrl + "\">link</a>");
                }
                catch (Exception e)
                {
                    Logger.AddLog(e.InnerException.Message, "ForgotPassword method");
                    return BadRequest();
                }
                //await UserManager.SendEmailAsync(user.Id, "Reset Password", $"Please reset your password by using this {code}");
                return Ok();
            }
            return BadRequest(ModelState);
        }

        #endregion

        #region Helpers

        private IAuthenticationManager Authentication
        {
            get { return Request.GetOwinContext().Authentication; }
        }

        private IHttpActionResult GetErrorResult(IdentityResult result)
        {
            if (result == null)
            {
                return InternalServerError();
            }

            if (!result.Succeeded)
            {
                if (result.Errors != null)
                {
                    foreach (string error in result.Errors)
                    {
                        ModelState.AddModelError("", error);
                    }
                }

                if (ModelState.IsValid)
                {
                    // No ModelState errors are available to send, so just return an empty BadRequest.
                    return BadRequest();
                }

                return BadRequest(ModelState);
            }

            return null;
        }

        private class ExternalLoginData
        {
            public string LoginProvider { get; set; }
            public string ProviderKey { get; set; }
            public string UserName { get; set; }

            public IList<Claim> GetClaims()
            {
                IList<Claim> claims = new List<Claim>();
                claims.Add(new Claim(ClaimTypes.NameIdentifier, ProviderKey, null, LoginProvider));

                if (UserName != null)
                {
                    claims.Add(new Claim(ClaimTypes.Name, UserName, null, LoginProvider));
                }

                return claims;
            }

            public static ExternalLoginData FromIdentity(ClaimsIdentity identity)
            {
                if (identity == null)
                {
                    return null;
                }

                Claim providerKeyClaim = identity.FindFirst(ClaimTypes.NameIdentifier);

                if (providerKeyClaim == null || String.IsNullOrEmpty(providerKeyClaim.Issuer)
                    || String.IsNullOrEmpty(providerKeyClaim.Value))
                {
                    return null;
                }

                if (providerKeyClaim.Issuer == ClaimsIdentity.DefaultIssuer)
                {
                    return null;
                }

                return new ExternalLoginData
                {
                    LoginProvider = providerKeyClaim.Issuer,
                    ProviderKey = providerKeyClaim.Value,
                    UserName = identity.FindFirstValue(ClaimTypes.Name)
                };
            }
        }

        private static class RandomOAuthStateGenerator
        {
            private static RandomNumberGenerator _random = new RNGCryptoServiceProvider();

            public static string Generate(int strengthInBits)
            {
                const int bitsPerByte = 8;

                if (strengthInBits % bitsPerByte != 0)
                {
                    throw new ArgumentException("strengthInBits must be evenly divisible by 8.", "strengthInBits");
                }

                int strengthInBytes = strengthInBits / bitsPerByte;

                byte[] data = new byte[strengthInBytes];
                _random.GetBytes(data);
                return HttpServerUtility.UrlTokenEncode(data);
            }
        }
        public List<string> CheckUserId(string UserId)
        {
            List<string> errors = new List<string>();
            if (string.IsNullOrEmpty(UserId))
            {
                errors.Add("User Id is Incorrect");
            }
            return errors;
        }

        #endregion
    }
}
