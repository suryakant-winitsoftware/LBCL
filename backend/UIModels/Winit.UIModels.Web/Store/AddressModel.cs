using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Winit.UIModels.Web.Store
{
    public class AddressModel : BaseModel
    {
        public string Type { get; set; }
        public string Name { get; set; }

        [Required (ErrorMessage ="Required")]
        public string Line1 { get; set; }
        public string Line2 { get; set; }
        public string Line3 { get; set; }
        public string Landmark { get; set; }
        public string Area { get; set; }
        public string SubArea { get; set; }
        public string ZipCode { get; set; }
        public string City { get; set; }
        public string CountryCode { get; set; }
        public string RegionCode { get; set; }
        public string Phone { get; set; }
        public string PhoneExtension { get; set; }
        public string Mobile1 { get; set; }
        public string Mobile2 { get; set; }
        public string Email { get; set; }
        public string Fax { get; set; }
        public string Latitude { get; set; }
        public string Longitude { get; set; }
        public decimal Altitude { get; set; }
        public string LinkedItemUID { get; set; }
        public string LinkedItemType { get; set; }
        public string Status { get; set; }
        public string StateCode { get; set; }
        public string TerritoryCode { get; set; }
        public string PAN { get; set; }
        public string AADHAR { get; set; }
        public string SSN { get; set; }
        public bool IsEditable { get; set; }
        public bool IsDefault { get; set; }
        public string Line4 { get; set; }
        public string Info { get; set; }
    }
}
