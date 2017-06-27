using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.Owin.Security;
using RealEstate.Common;
using RealEstate.DataAccesss;
using RealEstate.Models;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Web;
using System.Web.Http;

namespace RealEstate.Controllers
{
    [Authorize]
    public class UserController : ApiController
    {
        #region Authorization
        private const string LocalLoginProvider = "Local";
        private ApplicationUserManager _userManager;
        RealEstateContext db = new RealEstateContext();
        public UserController()
        {
        }

        public UserController(ApplicationUserManager userManager,
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

        #region usedMethod
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
                Logger.AddLog(e.InnerException.Message, "GetAllUsersList method", User.Identity.GetUserId());
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
                Logger.AddLog(e.InnerException.Message, "GetUserDetail method", User.Identity.GetUserId());
                return Request.CreateResponse(HttpStatusCode.BadRequest, e);
            }
        }

        [HostAuthentication(DefaultAuthenticationTypes.ExternalBearer)]
        [HttpGet]
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
                db.Entry(userDataInDB).State = EntityState.Modified;
                db.SaveChanges();
                return Request.CreateResponse(HttpStatusCode.OK, true);
            }
            catch (Exception e)
            {
                Logger.AddLog(e.InnerException.Message, "EditUserProfile method", User.Identity.GetUserId());
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
                db.Entry(userDataInDB).State = EntityState.Modified;
                db.SaveChanges();
                return Request.CreateResponse(HttpStatusCode.OK, true);
            }
            catch (Exception e)
            {
                Logger.AddLog(e.InnerException.Message, "DeleteUser method", User.Identity.GetUserId());
                return Request.CreateResponse(HttpStatusCode.BadRequest, e);
            }
        }

        #endregion

        protected override void Dispose(bool disposing)
        {
            if (disposing && _userManager != null)
            {
                _userManager.Dispose();
                _userManager = null;
            }

            base.Dispose(disposing);
        }

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

        #endregion

    }
}
