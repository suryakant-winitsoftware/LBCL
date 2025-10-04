using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Winit.UIModels.Web.Store
{
    public class CustomerInformationModel:BaseModel
    {
      
        public string CompanyUID { get; set; }

        [Required(ErrorMessage = "Code is required.")]
        public string Code { get; set; }

        [Required(ErrorMessage = "Number is required.")]
        public string Number { get; set; }

        [Required(ErrorMessage = "Name is required.")]
        public string Name { get; set; }

        [Required(ErrorMessage = "Alias Name is required.")]
        public string AliasName { get; set; }

        [Required(ErrorMessage = "Legal Name is required.")]
        public string LegalName { get; set; }

       
        public string Type { get; set; }
        public string RouteType { get; set; }

       
        public string BillToStoreUID { get; set; }

        
        public string ShipToStoreUID { get; set; }

        
        public string SoldToStoreUID { get; set; }

        
        public int Status { get; set; }

        
        public bool IsActive { get; set; }

       
        public string StoreClass { get; set; }

       
        public string StoreRating { get; set; }

       
        public bool IsBlocked { get; set; }

     
        public string BlockedReasonCode { get; set; }

        [Required(ErrorMessage = "Blocked Reason Description is required.")]
        public string BlockedReasonDescription { get; set; }

      
        public string CreatedByEmpUID { get; set; }

      
        public string CreatedByJobPositionUID { get; set; }

       
        public string CountryUID { get; set; }

       
        public string RegionUID { get; set; }

      
        public string CityUID { get; set; }

      
        public string Source { get; set; }

        [Required(ErrorMessage = "Outlet Name is required.")]
        public string OutletName { get; set; }

        
        public string BlockedByEmpUID { get; set; }

        [Required(ErrorMessage = "Arabic Name is required.")]
        public string ArabicName { get; set; }


        public bool IsTaxApplicable { get; set; } = false;

        [Required(ErrorMessage = "Tax Doc Number is required.")]
        public string TaxDocNumber { get; set; }

       
        public string SchoolWarehouse { get; set; }
        public bool VatOrGst { get; set; } = false;

       
        public string DayType { get; set; }

        
        public DateTime? SpecialDay { get; set; }

        
        public bool IsTaxDocVerified { get; set; }

      
        public decimal StoreSize { get; set; }

      
        public string ProspectEmpUID { get; set; }
        public string PriceType { get; set; }
        public string TaxKeyField { get; set; }
        public string StoreImage { get; set; }
        public bool IsVATQRCaptureMandatory { get; set; } = false;
        public string TaxType { get; set; }
    }
}
