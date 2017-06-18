using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace RealEstate.Models
{
    public class Action
    {
        public int ActionId { get; set; }
        public int UserId { get; set; }
        public int PropertyId { get; set; }
        public RequestStatus RequestStatus { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime ModifiedDate { get; set; }
    }

    public enum RequestStatus
    {
        Pending = 1,
        Approved = 2,
        Rejected = 3,
        Hold = 4
    }
}