using System;
using Winit.Modules.Base.Model;

namespace Winit.Modules.Merchandiser.Model.Interfaces
{
    public interface IProductSampling : IBaseModel
    {
        decimal SellingPrice { get; set; }
        int UnitUsed { get; set; }
        int UnitSold { get; set; }
        int NoOfCustomerApproached { get; set; }
        string BeatHistoryUID { get; set; }
        string RouteUID { get; set; }
        string JobPositionUID { get; set; }
        string EmpUID { get; set; }
        DateTime ExecutionTime { get; set; }
        string StoreUID { get; set; }
        string SKUUID { get; set; }
    }
} 