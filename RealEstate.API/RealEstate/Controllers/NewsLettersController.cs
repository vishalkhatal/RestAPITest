using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.Owin.Security;
using RealEstate.Common;
using RealEstate.DataAccesss;
using RealEstate.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;

namespace RealEstate.Controllers
{
    [Authorize]
    public class NewsLettersController : ApiController
    {
        #region token methods
        public NewsLettersController(ApplicationUserManager userManager,
            ISecureDataFormat<AuthenticationTicket> accessTokenFormat)
        {
            UserManager = userManager;
            AccessTokenFormat = accessTokenFormat;
        }
        private RealEstateContext db = new RealEstateContext();
        public NewsLettersController()
        {
        }
        private ApplicationUserManager _userManager;

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
        // GET: api/Newsletter/GetAllMyNewsLetter
        [HostAuthentication(DefaultAuthenticationTypes.ExternalBearer)]
        [HttpGet]
        public HttpResponseMessage GetAllMyNewsLetter()
        {
            try
            {
                ApplicationUser user = UserManager.FindById(User.Identity.GetUserId());
                var myNewsLetters = db.NewsLetters.Where(x => x.UserId == user.Id).ToList();
                return Request.CreateResponse(HttpStatusCode.OK, myNewsLetters);
            }
            catch (Exception e)
            {
                Logger.AddLog(e.InnerException.Message, "GetAllMyNewsLetter method");
                return Request.CreateResponse(HttpStatusCode.BadRequest, e);
            }
        }

        // GET: api/Newsletter/GetNewsLetterInfo/5
        [HostAuthentication(DefaultAuthenticationTypes.ExternalBearer)]
        [HttpGet]
        public HttpResponseMessage GetNewsLetterInfo(int id)
        {
            try
            {
                if (id > 0)
                {
                    ApplicationUser user = UserManager.FindById(User.Identity.GetUserId());
                    var newsLetterDetails = db.NewsLetters.Where(x => x.NewsLetterId == id && x.UserId == user.Id).FirstOrDefault();
                    return Request.CreateResponse(HttpStatusCode.OK, newsLetterDetails);
                }
                else
                {
                    return Request.CreateResponse(HttpStatusCode.BadRequest);
                }
            }
            catch (Exception e)
            {
                Logger.AddLog(e.InnerException.Message, "GetNewsLetterInfo method");
                return Request.CreateResponse(HttpStatusCode.BadRequest, e);
            }
        }

        // PUT: api/Newsletter/EditNewsLetter
        [HostAuthentication(DefaultAuthenticationTypes.ExternalBearer)]
        [HttpPut]
        public async System.Threading.Tasks.Task<IHttpActionResult> EditNewsLetter(NewsLetter newsLetter)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }
                var newsLetterInDb = db.NewsLetters.Where(x => x.NewsLetterId == newsLetter.NewsLetterId).FirstOrDefault();
                if (newsLetterInDb != null)
                {
                    newsLetterInDb.Title = newsLetter.Title;
                    newsLetterInDb.Decription = newsLetter.Decription;
                    newsLetterInDb.EventDate = newsLetter.EventDate;
                    newsLetterInDb.ModifiedDate = DateTime.UtcNow;
                    db.SaveChanges();
                    return Ok();
                }
                return NotFound();

            }
            catch (Exception e)
            {
                Logger.AddLog(e.InnerException.Message, "EditNewsLetter method");
                return BadRequest();
            }
        }

        // POST: api/Newsletter/AddNewsLetter
        [HostAuthentication(DefaultAuthenticationTypes.ExternalBearer)]
        [HttpPost]
        public async Task<IHttpActionResult> AddNewsLetter(NewsLetter newsLetter)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }
                ApplicationUser user = UserManager.FindById(User.Identity.GetUserId());
                newsLetter.UserId = user.Id;
                newsLetter.CreatedDate = DateTime.UtcNow; ;
                db.NewsLetters.Add(newsLetter);
                db.SaveChanges();
                return Ok();
            }
            catch (Exception e)
            {
                Logger.AddLog(e.InnerException.Message, "AddNewsLetter method");
                return BadRequest();
            }
        }

        // GET: api/NewsLetter/JoinNewsLetter
        [AllowAnonymous]
        [HttpGet]
        public HttpResponseMessage JoinNewsLetter(NewsLetterSubscriber subscriber)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return Request.CreateResponse(HttpStatusCode.BadRequest);
                }
                db.NewsLetterSubscribers.Add(subscriber);
                db.SaveChanges();
                return Request.CreateResponse(HttpStatusCode.OK);

            }
            catch (Exception e)
            {
                Logger.AddLog(e.InnerException.Message, "JoinNewsLetter method");
                return Request.CreateResponse(HttpStatusCode.BadRequest, e);
            }
        }

        [AllowAnonymous]
        [HttpGet]
        public HttpResponseMessage SendNewsLetter(NewsLetter newsLetter)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return Request.CreateResponse(HttpStatusCode.BadRequest);
                }
                var subscribers = db.NewsLetterSubscribers.ToList();
                #region send Newsletter to each subscriber
                foreach (var subscriber in subscribers)
                {
                    // send email here
                }
                #endregion
                return Request.CreateResponse(HttpStatusCode.OK);

            }
            catch (Exception e)
            {
                Logger.AddLog(e.InnerException.Message, "SendNewsLetter method");
                return Request.CreateResponse(HttpStatusCode.BadRequest, e);
            }
        }

    }
}
