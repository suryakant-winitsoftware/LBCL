using System;
using Winit.Modules.Base.Model;
using Winit.Modules.Initiative.Model.Interfaces;

namespace Winit.Modules.Initiative.Model.Classes
{
    public class AllocationMaster : BaseModel, IAllocationMaster
    {
        public string AllocationNo { get; set; }
        public string ActivityNo { get; set; }
        public string AllocationName { get; set; }
        public string AllocationDescription { get; set; }
        public decimal TotalAllocationAmount { get; set; }
        public decimal AvailableAllocationAmount { get; set; }
        public decimal ConsumedAmount { get; set; }
        public string Brand { get; set; }
        public string SalesOrgCode { get; set; }
        public string CustomerGroup { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public int? DaysLeft { get; set; }
        public bool IsActive { get; set; }
    }
}