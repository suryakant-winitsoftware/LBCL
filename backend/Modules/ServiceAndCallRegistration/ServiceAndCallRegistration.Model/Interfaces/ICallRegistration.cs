using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Winit.Modules.FileSys.Model.Interfaces;

namespace Winit.Modules.ServiceAndCallRegistration.Model.Interfaces
{
    public interface ICallRegistration : Winit.Modules.Base.Model.IBaseModel
    {
        string OrgUID { get; set; }
        public int? CustomerType { get; set; } // custType: 1=Individual, 2=Key Account
        public string CustomerName { get; set; } // custName
        public string ContactPerson { get; set; } // contactPerson
        public string EmailID { get; set; } // custEmail
        public string MobileNumber { get; set; } // custMobile
        public string Address { get; set; } // custAddress
        public string BrandCode { get; set; }
        public string Pincode { get; set; } // custPincode
        public string ServiceCallNo {  get; set; }
        public string CmiRelationshipNumber { get; set; } // cmiRelationShipNo (Optional)
        public string ProductCategoryCode { get; set; } // productCategoryCode
        public string ModelCode { get; set; } // Model Code (Optional)
        public int? ServiceType { get; set; } // serviceType: 1=Preventive Maintenance, 2=Breakdown, etc.
        public int? WarrantyStatus { get; set; } // custCoverageClaim: 1=Under Warranty, 2=Out Warranty, 3=AMC
        public string ResellerName { get; set; } // sellerName (Optional)
        public DateTime? PurchaseDate { get; set; } // purchaseDate (Optional)
        public string Remarks { get; set; } // custRemarks (Optional)
        public string DeviceId { get; set; } // deviceId (Optional)
        public string Username { get; set; } // userName
        public string Password { get; set; } // password
        public string ServiceRequestorMobile { get; set; } // serviveRequestorMobile (Optional)
        public int? RelationshipWithCmi { get; set; } // relationshipWithCMI: 1=Direct Customer, 2=Re-Seller (Optional)
        public List<IFileSys>? FileSys { get; set; } // Optional files


    }
}
