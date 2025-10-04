using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Winit.UIModels.Common.GST
{
    public class GSTINDetailsModel
    {
        public string? StateJurisdictionCode { get; set; }
        public string? LegalName { get; set; }
        public string? StateJurisdiction { get; set; }
        public string? Duty { get; set; }
        public string? CancellationDate { get; set; }
        public string? GSTIN { get; set; }
        public string? NatureOfBusinessActivity { get; set; }
        public string? LastUpdate { get; set; }
        public string? RegistrationDate { get; set; }
        public string? ConstitutionOfBusiness { get; set; }
        public string? TradeName { get; set; }
        public string? Status { get; set; }
        public string? CentralJurisdictionCode { get; set; }
        public string? CentralJurisdiction { get; set; }
        public string? AR_ADR_BuildingName { get; set; }
        public string? AR_ADR_Street { get; set; }
        public string? AR_ADR_District { get; set; }
        public string? AR_ADR_Location { get; set; }
        public string? AR_ADR_DoorNo { get; set; }
        public string? AR_ADR_State { get; set; }
        public string? AR_ADR_FloorNo { get; set; }
        public double AR_ADR_Latitude { get; set; }
        public double AR_ADR_Longitude { get; set; }
        public string? AR_ADR_Pincode { get; set; }
        public string? AR_NatureOfBusiness { get; set; }
        public string? PR_ADDR_BuildingName { get; set; }
        public string? PR_ADR_Street { get; set; }
        public string? PR_ADR_Location { get; set; }
        public string? PR_ADR_DoorNo { get; set; }
        public string? PR_ADR_State { get; set; }
        public string? PR_ADR_FloorNo { get; set; }
        public double PR_ADR_Latitude { get; set; }
        public double PR_ADR_Longitude { get; set; }
        public string? PR_ADR_Pincode { get; set; }
        public string? PR_NatureOfBusiness { get; set; }
        public string? AR_ADR_Locality { get; set; }
        public string? AR_ADR_Landmark { get; set; }
        public string? AR_ADR_GeoCodeLevel { get; set; }
        public string? PR_ADR_District { get; set; }
        public string? PR_ADR_Locality { get; set; }
        public string? PR_ADR_Landmark { get; set; }
        public string? PR_ADR_GeoCodeLevel { get; set; }
        public string? EInvoiceStatus { get; set; }
        public string? Address { get; set; }
        public bool IsPrimary { get; set; }
        public string? Email {get; set;}
        public long MobileNumber { get; set; }
    }
}
