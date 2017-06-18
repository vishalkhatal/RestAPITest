using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace RealEstate.API.Models
{
    public class Property
    {
        public int PropertyId { get; set; }
        public string UserId { get; set; }
        [Required]
        public string Title { get; set; }
        [Required]
        public string Decription { get; set; }
        [Required]
        public string Location { get; set; }
        [Required]
        public string City { get; set; }
        [Required]
        public string Country { get; set; }
        [Required]
        public decimal Cost { get; set; }
        public bool IsActive { get; set; }
        [Required]
        public PropertyType PropertyType { get; set; }
        [Required]
        public ResidentType ResidentType { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime ModifiedDate { get; set; }
        public RequestStatus Status { get; set; } // need to update this when admin takes action
        public DateTime StatusUpdatedDate { get; set; }
        // Navigation property 
        [Required]
        public virtual ICollection<PropertyPhoto> Photos { get; set; }
        public virtual ICollection<PropertyFeedBack> UserFeedBacks { get; set; }

    }

    public class PropertyPhoto
    {
        public int PropertyPhotoId { get; set; }
        public int PropertyId { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedDate { get; set; }
        public string FileName { get; set; }
        public string FileType { get; set; }
        public byte[] Photo { get; set; }

    }

    public enum PropertyType
    {
        ForSale = 1,
        Rent = 2
    }
    public enum ResidentType
    {
        Commericial = 1,
        Residential = 2
    }
}