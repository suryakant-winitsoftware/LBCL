using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Winit.Modules.Base.Model;
using Winit.Modules.CollectionModule.Model.Interfaces;

namespace Winit.Modules.CollectionModule.Model.Classes
{
    public class CollectionModule : BaseModel, ICollectionModule
    {
        public string SourceType { get; set; }
        public string SourceUID { get; set; }
        public string TargetUID { get; set; }
        public string ReferenceNumber { get; set; }
        public string OrgUID { get; set; }
        public string JobPositionUID { get; set; }
        public string DocumentType { get; set; }
        public bool IsManaged { get; set; }
        public decimal Amount { get; set; }
        public decimal UnSettledAmount { get; set; }
        public decimal PaidAmount { get; set; }
        public decimal BalanceAmount { get; set; }
        public bool IsActive { get; set; }
        public string StoreUID { get; set; }
        public DateTime? TransactionDate { get; set; }
        public DateTime? DueDate { get; set; }
        public string CompanyUID { get; set; }
        public string Code { get; set; }
        public string Number { get; set; }
        public string Name { get; set; }
        public string CodeName { get; set; }
        public string AliasName { get; set; }
        public string Type { get; set; }
        public string BillToStoreUID { get; set; }
        public string ShipToStoreUID { get; set; }
        public string SoldToStoreUID { get; set; }
        public int Status { get; set; }
        public string StoreClass { get; set; }
        public string StoreRating { get; set; }
        public bool IsBlocked { get; set; }
        public string BlockedReasonCode { get; set; }
        public string BlockedReasonDescription { get; set; }
        public string CreatedByEmpUID { get; set; }
        public string CreatedByJobPositionUID { get; set; }
        public string CountryUID { get; set; }
        public string RegionUID { get; set; }
        public string CityUID { get; set; }
        public string Source { get; set; }
    }
}
