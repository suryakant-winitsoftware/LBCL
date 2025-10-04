using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Winit.Modules.Store.Model.Interfaces
{
    public interface IStore : Base.Model.IBaseModel
    {
        [Column("company_uid")]
        public string CompanyUID { get; set; }

        [Column("code")]
        public string Code { get; set; }

        [Column("number")]
        public string Number { get; set; }

        [Column("name")]
        public string Name { get; set; }

        [Column("alias_name")]
        public string AliasName { get; set; }

        [Column("legal_name")]
        public string LegalName { get; set; }

        [Column("type")]
        public string Type { get; set; }

        [Column("price_type")]
        public string PriceType { get; set; }

        [Column("route_type")]
        public string RouteType { get; set; }

        [Column("bill_to_store_uid")]
        public string BillToStoreUID { get; set; }

        [Column("ship_to_store_uid")]
        public string ShipToStoreUID { get; set; }

        [Column("sold_to_store_uid")]
        public string SoldToStoreUID { get; set; }

        [Column("status")]
        public string Status { get; set; }

        [Column("is_active")]
        public bool IsActive { get; set; }

        [Column("store_class")]
        public string StoreClass { get; set; }

        [Column("store_rating")]
        public string StoreRating { get; set; }

        [Column("is_blocked")]
        public bool IsBlocked { get; set; }

        [Column("blocked_reason_code")]
        public string BlockedReasonCode { get; set; }

        [Column("blocked_reason_description")]
        public string BlockedReasonDescription { get; set; }

        [Column("created_by_emp_uid")]
        public string CreatedByEmpUID { get; set; }

        [Column("created_by_job_position_uid")]
        public string CreatedByJobPositionUID { get; set; }

        [Column("country_uid")]
        public string CountryUID { get; set; }

        [Column("region_uid")]
        public string RegionUID { get; set; }

        [Column("state_uid")]
        public string StateUID { get; set; }

        [Column("location_uid")]
        public string LocationUID { get; set; }

        [Column("city_uid")]
        public string CityUID { get; set; }

        [Column("source")]
        public string Source { get; set; }

        [Column("outlet_name")]
        public string OutletName { get; set; }

        [Column("blocked_by_emp_uid")]
        public string BlockedByEmpUID { get; set; }

        [Column("arabic_name")]
        public string ArabicName { get; set; }

        [Column("is_tax_applicable")]
        public bool IsTaxApplicable { get; set; }

        [Column("tax_doc_number")]
        public string TaxDocNumber { get; set; }

        [Column("school_warehouse")]
        public string SchoolWarehouse { get; set; }

        [Column("day_type")]
        public string DayType { get; set; }

        [Column("special_day")]
        public DateTime? SpecialDay { get; set; }

        [Column("is_tax_doc_verified")]
        public bool IsTaxDocVerified { get; set; }

        [Column("store_size")]
        public decimal StoreSize { get; set; }

        [Column("prospect_emp_uid")]
        public string ProspectEmpUID { get; set; }

        [Column("tax_key_field")]
        public string TaxKeyField { get; set; }

        [Column("store_image")]
        public string StoreImage { get; set; }

        [Column("is_vat_qr_capture_mandatory")]
        public bool IsVATQRCaptureMandatory { get; set; }

        [Column("tax_type")]
        public string TaxType { get; set; }

        [Column("franchisee_org_uid")]
        public string FranchiseeOrgUID { get; set; }

        [Column("broad_classification")]
        public string BroadClassification { get; set; }

        [Column("classfication_type")]
        public string ClassficationType { get; set; }

        [Column("reporting_emp_uid")]
        public string ReportingEmpUID { get; set; }

        // Additional fields
        [Column("is_new")]
        public bool IsNew { get; set; }

        [Column("total_out_standings")]
        public int TotalOutStandings { get; set; }

        [Column("email")]
        public string Email { get; set; }

        [Column("store_number")]
        public string StoreNumber { get; set; }

        [Column("customer_code")]
        public string CustomerCode { get; set; }

        [Column("gst_no")]
        public string GSTNo { get; set; }

        [Column("trade_name")]
        public string TradeName { get; set; }

        [Column("area")]
        public string Area { get; set; }

        [Column("org_parent_uid")]
        public string OrgParentUID { get; set; }

        [Column("branch_uid")]
        public string BranchUID { get; set; }

        [Column("shipping_address_uid")]
        public string ShippingAddressUID { get; set; }
        public string? LocationLabel { get; set; }
        public string? LocationJson { get; set; }
        public bool IsAsmMappedByCustomer { get; set; }
        public bool IsAvailableToUse { get; set; }
        public bool IsApprovalCreated { get; set; }

        string Contact { get; set; }

    }
}
