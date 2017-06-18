using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace RealEstate.Models
{  
    public class PropertyFeedBack
    {
        public int PropertyFeedBackId { get; set; }
        public string PropertyId { get; set; }
        public string Message { get; set; }
        public DateTime SentDate { get; set; }
        public string SenderName { get; set; }
        public string SenderMobileNo { get; set; }
        public string SenderEmailAddress { get; set; }
    }

}