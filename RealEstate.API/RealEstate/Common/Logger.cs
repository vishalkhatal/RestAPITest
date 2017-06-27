using RealEstate.DataAccesss;
using RealEstate.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace RealEstate.Common
{
    public static class Logger
    {
        public static void AddLog(string message,string location,string userId=null)
        {
            RealEstateContext db = new DataAccesss.RealEstateContext();
            ApplicationLog log = new Models.ApplicationLog();
            log.CreatdAt = DateTime.UtcNow;
            log.Message = message;
            log.Location = location;
            log.UserIDs = userId;
            db.ApplicationLogs.Add(log);
            db.SaveChanges();

        }
    }
}