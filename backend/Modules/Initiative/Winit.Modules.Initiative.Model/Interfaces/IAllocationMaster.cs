using System;
using Winit.Modules.Base.Model;

namespace Winit.Modules.Initiative.Model.Interfaces
{
    public interface IAllocationMaster : IBaseModel
    {
        string AllocationNo { get; set; }
        string ActivityNo { get; set; }
        string AllocationName { get; set; }
        string AllocationDescription { get; set; }
        decimal TotalAllocationAmount { get; set; }
        decimal AvailableAllocationAmount { get; set; }
        decimal ConsumedAmount { get; set; }
        string Brand { get; set; }
        string SalesOrgCode { get; set; }
        string CustomerGroup { get; set; }
        DateTime StartDate { get; set; }
        DateTime EndDate { get; set; }
        int? DaysLeft { get; set; }
        bool IsActive { get; set; }
    }
}