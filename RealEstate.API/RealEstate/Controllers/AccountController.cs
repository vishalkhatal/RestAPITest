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
namespace RealEstate.Controllers
{
    [Authorize]
    [RoutePrefix("api/Account")]
    public class AccountController : ApiController
    {
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

        #region Used methods

        // GET: api/Account/GetAllUsersList
        [HostAuthentication(DefaultAuthenticationTypes.ExternalBearer)]
        [HttpGet]
        public IHttpActionResult GetAllUsersList()
        {
            try
            {
                ApplicationUser user = UserManager.FindById(User.Identity.GetUserId());
                var userDetails = db.Users.Where(x => x.UserId == user.Id).Select(y => y).FirstOrDefault();
                if (userDetails.UserType == UserType.BuilderAdmin)
                {
                    var allusersList = db.Users.ToList();
                    return Ok(allusersList);

                }
                else
                {
                    return Unauthorized();

                }
            }
            catch (Exception e)
            {
                return BadRequest();
            }
        }

        // GET: api/Account/GetUserDetail/5
        [HostAuthentication(DefaultAuthenticationTypes.ExternalBearer)]
        [HttpGet]
        public HttpResponseMessage GetUserDetail()
        {
            try
            {
                ApplicationUser user = UserManager.FindById(User.Identity.GetUserId());
                var userDetails = db.Users.Where(x => x.UserId == user.Id).Select(y => y).FirstOrDefault();
                return Request.CreateResponse(HttpStatusCode.OK, userDetails);
            }
            catch (Exception e)
            {
                return Request.CreateResponse(HttpStatusCode.BadRequest, e);
            }
        }

        [HostAuthentication(DefaultAuthenticationTypes.ExternalBearer)]
        [HttpGet]
        public HttpResponseMessage GetUserInfo(string id)
        {
            try
            {
                if (string.IsNullOrEmpty(id))
                {
                    return Request.CreateResponse(HttpStatusCode.BadRequest);
                }
                var userDetails = db.Users.Where(x => x.UserId == id).Select(y => y).FirstOrDefault();
                return Request.CreateResponse(HttpStatusCode.OK, userDetails);
            }
            catch (Exception e)
            {
                return Request.CreateResponse(HttpStatusCode.BadRequest, e);
            }
        }

        // GET: api/Account/GetUserNotification/5
        //public HttpResponseMessage GetUserNotification(string id)
        //{
        //    ResponseWithObject response = new ResponseWithObject();
        //    try
        //    {
        //        ApplicationUser userDetail = UserManager.FindById(User.Identity.GetUserId());
        //        List<string> notifications = new List<string>();
        //        if (userDetail != null)
        //        {
        //            if (userDetail.User.Subscription == Subscription.Guest)
        //            {
        //                TimeSpan difference = DateTime.Today - userDetail.User.CreatedAt; //create TimeSpan object

        //                int days = (int)Math.Ceiling(difference.TotalDays); //Extract days, counting parts of a day as a full day (rounding up).
        //                if (days < 30)
        //                {
        //                    notifications.Add(string.Format("Last {0} days are remaining in your subscriptions.Hurry up !! ", days));

        //                }
        //            }
        //            else
        //            {
        //                notifications.Add("No new notifications !!! ");
        //            }

        //        }

        //        response.IsSuccess = true;
        //        response.Response = notifications;
        //    }
        //    catch (Exception e)
        //    {
        //        return Request.CreateResponse(HttpStatusCode.BadRequest, e);
        //    }
        //    return Request.CreateResponse(HttpStatusCode.OK, response.Response);
        //}


        // POST: api/Users

        public HttpResponseMessage EditUserProfile(User user)
        {
            try
            {
                ApplicationUser userDetail = UserManager.FindById(User.Identity.GetUserId());
                var userDataInDB = db.Users.Where(x => x.UserId == userDetail.Id).FirstOrDefault();
                userDataInDB.FirstName = user.FirstName;
                userDataInDB.MiddleName = user.MiddleName;
                userDataInDB.LastName = user.LastName;
                userDataInDB.Address = user.Address;
                userDataInDB.City = user.City;
                userDataInDB.Country = user.Country;
                userDataInDB.MobileNo = user.MobileNo;
                userDataInDB.Subscription = user.Subscription;
                userDataInDB.ProfilePhoto = user.ProfilePhoto;
                db.SaveChanges();
                return Request.CreateResponse(HttpStatusCode.OK, true);
            }
            catch (Exception e)
            {
                return Request.CreateResponse(HttpStatusCode.BadRequest, e);
            }
        }

        [HostAuthentication(DefaultAuthenticationTypes.ExternalBearer)]
        [HttpGet]
        public HttpResponseMessage DeleteUser(string id)
        {
            try
            {
                if (string.IsNullOrEmpty(id))
                {
                    return Request.CreateResponse(HttpStatusCode.BadRequest);
                }
                var userDataInDB = db.Users.Where(x => x.UserId == id).FirstOrDefault();
                userDataInDB.IsActive = false;
                db.SaveChanges();
                return Request.CreateResponse(HttpStatusCode.OK, true);
            }
            catch (Exception e)
            {
                return Request.CreateResponse(HttpStatusCode.BadRequest, e);
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing && _userManager != null)
            {
                _userManager.Dispose();
                _userManager = null;
            }

            base.Dispose(disposing);
        }
        // POST api/Account/ChangePassword
        [Route("ChangePassword")]
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

        // POST api/Account/Register
        [AllowAnonymous]
        [Route("Register")]
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

                var callbackUrl = System.Configuration.ConfigurationManager.AppSettings["AppBaseAddress"] + "Account/ConfirmEmail?userId=" + userDetail.Id + "&code=" + code;

                await UserManager.SendEmailAsync(user.Id,
                   "Confirm your account",
                   "Please confirm your account by clicking: <a href=\""
                                                   + callbackUrl + "\">link</a>");

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
                return BadRequest();

            }
        }

        [AllowAnonymous]
        [Route("ConfirmEmail")]
        public async Task<IHttpActionResult> ConfirmEmail(ConfirmAccountViewModel model)
        {
            if (model.userId == null || model.Code == null)
            {
                return BadRequest();
            }
            var code = model.Code.Replace(" ", "+");
            var result = await UserManager.ConfirmEmailAsync(model.userId, code);
            if (result.Succeeded)
            {
                return Ok();
            }
            else
                return BadRequest();
        }


        // POST api/Account/Register
        [AllowAnonymous]
        [Route("Login")]
        public HttpResponseMessage Login(LoginViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return Request.CreateResponse(HttpStatusCode.BadRequest, false);
            }


            var userDetail = UserManager.FindByName(model.UserName);
            if (userDetail != null && userDetail.EmailConfirmed)
            {
                if (UserManager.CheckPassword(userDetail, model.Password))
                {
                    var userInfo = db.Users.Where(x => x.UserId == userDetail.Id).FirstOrDefault();
                    return Request.CreateResponse(HttpStatusCode.OK, userInfo);
                }
            }


            return Request.CreateResponse(HttpStatusCode.NotFound, false);
        }


        [HttpPost]
        [AllowAnonymous]
        [Route("ForgotPassword")]
        public async Task<IHttpActionResult> ForgotPassword(ForgotPasswordViewModel model)
        {
            if (ModelState.IsValid)
            {
                var user = await UserManager.FindByNameAsync(model.Email);
                // If user has to activate his email to confirm his account, the use code listing below
                if (user == null || !(await UserManager.IsEmailConfirmedAsync(user.Id)))
                {
                    return Ok();
                }
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
