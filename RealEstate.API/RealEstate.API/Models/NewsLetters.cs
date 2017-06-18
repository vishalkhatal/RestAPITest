using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace RealEstate.API.Models
{
    public class NewsLetter
    {
        public int NewsLetterId { get; set; }
        public string UserId { get; set; }
        public string Title { get; set; }
        public string Decription { get; set; }
        public DateTime? EventDate { get; set; }
        public string Location { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime ModifiedDate { get; set; }

    }
    public class NewsLetterSubscriber
    {
        public int NewsLetterSubscriberId { get; set; }
        public string MobileNo { get; set; }
        public string EmailId { get; set; }
        public int BuilderId { get; set; }
        public bool IsActive { get; set; }
        public DateTime ModifiedDate { get; set; }
    }
}