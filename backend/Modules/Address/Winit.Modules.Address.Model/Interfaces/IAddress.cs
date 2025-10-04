using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Winit.Modules.Base.Model;
using Winit.Modules.Location.Model.Classes;

namespace Winit.Modules.Address.Model.Interfaces
{
    //public interface IAddress: IBaseModel
    //{
    //    public string Type { get; set; }
    //    public string Name { get; set; }
    //    public string Line1 { get; set; }
    //    public string Line2 { get; set; }
    //    public string Line3 { get; set; }
    //    public string Landmark { get; set; }
    //    public string Area { get; set; }
    //    public string SubArea { get; set; }
    //    public string ZipCode { get; set; }
    //    public string City { get; set; }
    //    public string CountryCode { get; set; }
    //    public string RegionCode { get; set; }
    //    public string Phone { get; set; }
    //    public string PhoneExtension { get; set; }
    //    public string Mobile1 { get; set; }
    //    public string Mobile2 { get; set; }
    //    public string Email { get; set; }
    //    public string Fax { get; set; }
    //    public string Latitude { get; set; }
    //    public string Longitude { get; set; }
    //    public decimal Altitude { get; set; }
    //    public string LinkedItemUID { get; set; }
    //    public string LinkedItemType { get; set; }
    //    public string Status { get; set; }
    //    public string StateCode { get; set; }
    //    public string TerritoryCode { get; set; }
    //    public string PAN { get; set; }
    //    public string AADHAR { get; set; }
    //    public string SSN { get; set; }
    //    public bool IsEditable { get; set; }
    //    public bool IsDefault { get; set; }
    //    public string Line4 { get; set; }
    //    public string Info { get; set; }
    //    public string Depot { get; set; }
    //    public string? LocationUID { get; set; }
    //    public string? LocationLabel { get; set; }
    //    public string? LocationJson{ get; set; }
    //    public string? CustomField1 { get; set; }
    //    public string? CustomField2 { get; set; }
    //    public string? CustomField3 { get; set; }
    //    public string? CustomField4 { get; set; }
    //    public string? CustomField5 { get; set; }
    //    public string? CustomField6 { get; set; }
    //    List<LocationMaster>? LocationMasters { get; set; }

    //    //By Selva
    //    public string? HusbandName { get; set; }
    //    public string? FatherName { get; set; }
    //    public string? GSTNo { get; set; }
    //    public string? MuncipalRegNo { get; set; }
    //    public string? ESICRegNo { get; set; }
    //    public string? PFRegNo { get; set; }
    //    public string? SalesOfficeUID { get; set; }
    //    public string? State { get; set; }
    //    public string? BranchUID { get; set; }
    //    public string? Locality { get; set; }
    //    public string SectionName { get; set; }
    //    public string OrgUnitUID { get; set; }
    //}
    public interface IAddress : IBaseModel
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
      
    }
}

