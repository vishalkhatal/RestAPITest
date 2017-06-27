using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Web.Http.Description;
using RealEstate.DataAccesss;
using RealEstate.Models;
using Microsoft.Owin.Security;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.AspNet.Identity;
using System.Threading.Tasks;
using RealEstate.Common;

namespace RealEstate.Controllers
{
    [Authorize]
    public class PropertiesController : ApiController
    {
        #region token methods
        public PropertiesController(ApplicationUserManager userManager,
            ISecureDataFormat<AuthenticationTicket> accessTokenFormat)
        {
            UserManager = userManager;
            AccessTokenFormat = accessTokenFormat;
        }
        private RealEstateContext db = new RealEstateContext();
        public PropertiesController()
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

        // GET: api/Property/GetAllMyProperties
        [HostAuthentication(DefaultAuthenticationTypes.ExternalBearer)]
        [HttpGet]
        public HttpResponseMessage GetAllMyProperties()
        {
            try
            {
                ApplicationUser user = UserManager.FindById(User.Identity.GetUserId());
                var builderProperties = db.Properties.Where(x => x.UserId == user.Id)
                                        .Include(z=>z.Photos)
                                        .Include(y=>y.UserFeedBacks)
                                        .ToList();
                return Request.CreateResponse(HttpStatusCode.OK, builderProperties);
            }
            catch (Exception e)
            {
                Logger.AddLog(e.InnerException.Message, "GetAllMyProperties method", User.Identity.GetUserId());
                return Request.CreateResponse(HttpStatusCode.BadRequest, e);
            }
        }

        // GET: api/Property/GetPropertyInfo/5
        [HostAuthentication(DefaultAuthenticationTypes.ExternalBearer)]
        [HttpGet]
        public HttpResponseMessage GetPropertyInfo(int id)
        {
            try
            {
                if (id > 0)
                {
                    var propDetails = db.Properties
                                        .Include(p => p.Photos)
                                        .Include(p => p.UserFeedBacks)
                                        .Where(x => x.PropertyId == id).FirstOrDefault();
                    return Request.CreateResponse(HttpStatusCode.OK, propDetails);
                }
                else
                {
                    return Request.CreateResponse(HttpStatusCode.BadRequest);
                }
            }
            catch (Exception e)
            {
                Logger.AddLog(e.InnerException.Message, "GetPropertyInfo method", User.Identity.GetUserId());
                return Request.CreateResponse(HttpStatusCode.BadRequest, e);
            }
        }

        // PUT: api/Property/EditProperty
        [HostAuthentication(DefaultAuthenticationTypes.ExternalBearer)]
        [HttpPut]
        public async Task<IHttpActionResult> EditProperty(Property property)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }
                var propInDb = db.Properties.Where(x => x.PropertyId == property.PropertyId).FirstOrDefault();
                if (propInDb != null)
                {
                    propInDb.PropertyType = property.PropertyType;
                    propInDb.ResidentType = property.ResidentType;
                    propInDb.Status = property.Status;
                    propInDb.Title = property.Title;

                    propInDb.Decription = property.Decription;
                    propInDb.IsActive = property.IsActive;
                    propInDb.Photos = property.Photos;
                    propInDb.UserFeedBacks = property.UserFeedBacks;
                    propInDb.ModifiedDate = DateTime.UtcNow;
                    db.SaveChanges();
                    return Ok();
                }
                return NotFound();

            }
            catch (Exception e)
            {
                Logger.AddLog(e.InnerException.Message, "EditProperty method", User.Identity.GetUserId());
                return BadRequest();
            }
        }

        // POST: api/Property/RegisterProperty
        [HostAuthentication(DefaultAuthenticationTypes.ExternalBearer)]
        [HttpPost]
        public async Task<IHttpActionResult> RegisterProperty(Property property)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }
                ApplicationUser user = UserManager.FindById(User.Identity.GetUserId());
                property.UserId = user.Id;
                property.Status = RequestStatus.Pending;
                property.CreatedDate = DateTime.UtcNow;
                db.Properties.Add(property);
                db.SaveChanges();
                return Ok();
            }
            catch (Exception e)
            {
                Logger.AddLog(e.InnerException.Message, "RegisterProperty method", User.Identity.GetUserId());
                return BadRequest();
            }
        }

        // GET: api/Property/GetAllProperties
        [AllowAnonymous]
        [HttpGet]
        public HttpResponseMessage GetTopProperties()
        {
            try
            {
                var topProperties = db.Properties
                                    .Include(p => p.Photos)
                                    .Include(y=>y.UserFeedBacks)
                                    .Where(x => x.Status == RequestStatus.Approved)
                                    .Take(3).ToList();
                return Request.CreateResponse(HttpStatusCode.OK, topProperties);
            }
            catch (Exception e)
            {
                Logger.AddLog(e.InnerException.Message, "GetTopProperties method", User.Identity.GetUserId());
                return Request.CreateResponse(HttpStatusCode.BadRequest, e);
            }
        }

        // GET: api/Property/GetAllPropertiesByLocation
        [AllowAnonymous]
        [HttpGet]
        public HttpResponseMessage GetAllPropertiesByLocation(string city, string country)
        {
            try
            {
                var allProperties = db.Properties
                                    .Include(p => p.Photos)
                                    .Include(y=>y.UserFeedBacks)
                                    .Where(x => x.Status == RequestStatus.Approved 
                                    && x.Country == country 
                                    && x.City == city)
                                    .ToList();
                return Request.CreateResponse(HttpStatusCode.OK, allProperties);
            }
            catch (Exception e)
            {
                Logger.AddLog(e.InnerException.Message, "GetAllPropertiesByLocation method", User.Identity.GetUserId());
                return Request.CreateResponse(HttpStatusCode.BadRequest, e);
            }
        }

        // GET: api/Property/GetAllPropertiesByLocation
        [AllowAnonymous]
        [HttpGet]
        public HttpResponseMessage GetPropertiesBySearchCriteria(string city, string country, decimal maxcost, decimal mincost, int propertyType)
        {
            try
            {
                var allProperties = db.Properties
                                    .Include(p => p.Photos)
                                    .Include(p => p.UserFeedBacks)
                                    .Where(x => x.Status == RequestStatus.Approved 
                                                && x.City == city
                                                && x.Country == country
                                                && x.Cost <= maxcost &&
                                                x.Cost >= mincost && 
                                                x.PropertyType == (PropertyType)propertyType
                                           )
                                    .ToList();
                return Request.CreateResponse(HttpStatusCode.OK, allProperties);
            }
            catch (Exception e)
            {
                Logger.AddLog(e.InnerException.Message, "GetPropertiesBySearchCriteria method", User.Identity.GetUserId());
                return Request.CreateResponse(HttpStatusCode.BadRequest, e);
            }
        }
              
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }

        private bool PropertyExists(int id)
        {
            return db.Properties.Count(e => e.PropertyId == id) > 0;
        }
    }
}