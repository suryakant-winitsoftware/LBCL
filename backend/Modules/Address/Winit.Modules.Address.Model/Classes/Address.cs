using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using Winit.Modules.Address.Model.Interfaces;
using Winit.Modules.Base.Model;
using Winit.Modules.Location.Model.Classes;

namespace Winit.Modules.Address.Model.Classes
{
    public class Address:BaseModel,IAddress
    {
        [Column("type")]
        public string Type { get; set; }

        [Column("name")]
        public string Name { get; set; }

        [Column("line1")]
        public string Line1 { get; set; }

        [Column("line2")]
        public string Line2 { get; set; }

        [Column("line3")]
        public string Line3 { get; set; }

        [Column("landmark")]
        public string Landmark { get; set; }

        [Column("area")]
        public string Area { get; set; }

        [Column("sub_area")]
        public string SubArea { get; set; }

        [Column("zip_code")]
        public string ZipCode { get; set; }

        [Column("city")]
        public string City { get; set; }

        [Column("country_code")]
        public string CountryCode { get; set; }

        [Column("region_code")]
        public string RegionCode { get; set; }

        [Column("phone")]
        public string Phone { get; set; }

        [Column("phone_extension")]
        public string PhoneExtension { get; set; }

        [Column("mobile1")]
        public string Mobile1 { get; set; }

        [Column("mobile2")]
        public string Mobile2 { get; set; }

        [Column("email")]
        public string Email { get; set; }

        [Column("fax")]
        public string Fax { get; set; }

        [Column("latitude")]
        public string Latitude { get; set; }

        [Column("longitude")]
        public string Longitude { get; set; }

        [Column("altitude")]
        public decimal Altitude { get; set; }

        [Column("linked_item_uid")]
        public string LinkedItemUID { get; set; }

        [Column("linked_item_type")]
        public string LinkedItemType { get; set; }

        [Column("status")]
        public string Status { get; set; }

        [Column("state_code")]
        public string StateCode { get; set; }

        [Column("territory_code")]
        public string TerritoryCode { get; set; }

        [Column("pan")]
        public string PAN { get; set; }

        [Column("aadhar")]
        public string AADHAR { get; set; }

        [Column("ssn")]
        public string SSN { get; set; }

        [Column("is_editable")]
        public bool IsEditable { get; set; }

        [Column("is_default")]
        public bool IsDefault { get; set; }

        [Column("line4")]
        public string Line4 { get; set; }

        [Column("info")]
        public string Info { get; set; }

        [Column("depot")]
        public string Depot { get; set; }

        [Column("location_uid")]
        public string? LocationUID { get; set; }

        [Column("location_label")]
        public string? LocationLabel { get; set; }

        [Column("location_json")]
        public string? LocationJson { get; set; }

        [Column("custom_field1")]
        public string? CustomField1 { get; set; }

        [Column("custom_field2")]
        public string? CustomField2 { get; set; }

        [Column("custom_field3")]
        public string? CustomField3 { get; set; }

        [Column("custom_field4")]
        public string? CustomField4 { get; set; }

        [Column("custom_field5")]
        public string? CustomField5 { get; set; }

        [Column("custom_field6")]
        public string? CustomField6 { get; set; }

        public List<LocationMaster>? LocationMasters { get; set; }

        // By Selva
        [Column("husband_name")]
        public string? HusbandName { get; set; }

        [Column("father_name")]
        public string? FatherName { get; set; }

        [Column("gst_no")]
        public string? GSTNo { get; set; }

        [Column("muncipal_reg_no")]
        public string? MuncipalRegNo { get; set; }

        [Column("esic_reg_no")]
        public string? ESICRegNo { get; set; }

        [Column("pf_reg_no")]
        public string? PFRegNo { get; set; }

        [Column("sales_office_uid")]
        public string? SalesOfficeUID { get; set; }

        [Column("state")]
        public string? State { get; set; }

        [Column("branch_uid")]
        public string? BranchUID { get; set; }

        [Column("locality")]
        public string? Locality { get; set; }

        [Column("org_unit_uid")]
        public string OrgUnitUID { get; set; }

        [Column("section_name")]
        public string SectionName { get; set; }

        [Column("custom_filed3")]
        public string SiteNo { get; set; }
        [JsonIgnore]
        public Dictionary<string, List<string>>? RequestUIDDictionary { get; set; }
    }
}

