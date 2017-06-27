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
using System.Threading.Tasks;
using System.Web.Http;

namespace RealEstate.Controllers
{
    [Authorize]
    public class ActionsController : ApiController
    {
        #region token methods
        public ActionsController(ApplicationUserManager userManager,
            ISecureDataFormat<AuthenticationTicket> accessTokenFormat)
        {
            UserManager = userManager;
            AccessTokenFormat = accessTokenFormat;
        }
        private RealEstateContext db = new RealEstateContext();
        public ActionsController()
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
        // GET: api/Action/GetActions
        [HostAuthentication(DefaultAuthenticationTypes.ExternalBearer)]
        [HttpGet]
        public HttpResponseMessage GetActions(string actionType = "1")
        {
            try
            {
                ApplicationUser user = UserManager.FindById(User.Identity.GetUserId());
                var userDetails = db.Users.Where(x => x.UserId == user.Id).Select(y => y).FirstOrDefault();
                if (userDetails.UserType == UserType.BuilderAdmin)
                {
                    var status = RequestStatus.Pending;
                    if (actionType == "2")
                        status = RequestStatus.Hold;
                    else if (actionType == "3")
                        status = RequestStatus.Approved;
                    else if (actionType == "4")
                        status = RequestStatus.Rejected;

                    var tasks = db.Properties.Where(x => x.Status == status)
                                .Include(y=>y.Photos)
                                .Include(z=>z.UserFeedBacks)
                                .ToList();
                    return Request.CreateResponse(HttpStatusCode.OK,tasks);
                }
                return Request.CreateResponse(HttpStatusCode.Unauthorized);

            }
            catch (Exception e)
            {
                Logger.AddLog(e.InnerException.Message, "GetAction method", User.Identity.GetUserId());
                return Request.CreateResponse(HttpStatusCode.BadRequest, e);
            }
        }

        // PUT: api/Action/PerformAction
        [HostAuthentication(DefaultAuthenticationTypes.ExternalBearer)]
        [HttpPut]
        public HttpResponseMessage PerformAction(PropertyActionViewModel model)
        {
            try
            {
                ApplicationUser user = UserManager.FindById(User.Identity.GetUserId());
                var userDetails = db.Users.Where(x => x.UserId == user.Id).Select(y => y).FirstOrDefault();
                if (userDetails.UserType == UserType.BuilderAdmin)
                {
                    if (ModelState.IsValid)
                    {
                        foreach (var propertyId in model.propertyIds)
                        {
                            var propInDb = db.Properties.Where(x => x.PropertyId == propertyId).FirstOrDefault();
                            propInDb.Status = model.Status;
                            propInDb.StatusUpdatedDate = DateTime.Now;
                        }
                        db.SaveChanges();
                    }
                    else
                    {
                        return Request.CreateResponse(HttpStatusCode.BadRequest, true);
                    }
                    return Request.CreateResponse(HttpStatusCode.OK, true);
                }
                return Request.CreateResponse(HttpStatusCode.Unauthorized);

            }
            catch (Exception e)
            {
                Logger.AddLog(e.InnerException.Message, "PerformAction method", User.Identity.GetUserId());
                return Request.CreateResponse(HttpStatusCode.BadRequest, e);
            }
        }
    }
}
