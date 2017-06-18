using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace RealEstate.Models
{
    public class User
    {
        public string UserId { get; set; }
        public string FirstName { get; set; }
        public string MiddleName { get; set; }
        public string LastName { get; set; }
        public bool IsActive { get; set; }
        public UserType UserType { get; set; }
        public string Address { get; set; }
        public string City { get; set; }
        public string Country { get; set; }
        public string UserName { get; set; }
        public string Password { get; set; }
        public string Email { get; set; }
        public string MobileNo { get; set; }
        public string LandLineNo { get; set; }
        public Subscription Subscription { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime ModifiedAt { get; set; }
        public ProfilePhoto ProfilePhoto { get; set; }
        public virtual ICollection<Property> Property { get; set; }
        public virtual ICollection<NewsLetter> NewsLetters { get; set; }
        public virtual ICollection<Action> Actions { get; set; }// approve,reject,hold
        public virtual ICollection<Message> Inbox { get; set; }// enquiries
    }
    public class ProfilePhoto
    {
        public string ProfilePhotoId { get; set; }
        public string FileName { get; set; }
        public string FileType { get; set; }
        public byte[] Photo { get; set; }
        public DateTime CreatedDate { get; set; }
    }
    public enum UserType
    {
        BuilderAdmin = 1,
        Builder = 2,
        Guest = 3,
    }
    public enum Subscription
    {
        Guest = 1,
        Monthly = 2,
        Yearly = 3,
        Permanant = 4
    }

}