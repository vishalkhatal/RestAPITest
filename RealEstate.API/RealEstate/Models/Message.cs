using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace RealEstate.Models
{
    public class Message
    {
        public int MessageId { get; set; }
        public string UserId { get; set; }
        public string Text { get; set; }
        public DateTime SentDate { get; set; }
        public string SenderName { get; set; }
        public string SenderMobileNo { get; set; }
        public string SenderEmailAddress { get; set; }
    }

}