using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace RealEstate.Models
{
    public class ApplicationLog
    {
        public string Message { get; set; }
        public string UserIDs { get; set; }
        public string Location { get; set; }
        public DateTime CreatdAt { get; set; }
    }
}