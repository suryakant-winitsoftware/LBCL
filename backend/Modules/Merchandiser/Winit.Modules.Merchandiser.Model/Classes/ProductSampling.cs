using System;
using Winit.Modules.Base.Model;
using Winit.Modules.Merchandiser.Model.Interfaces;

namespace Winit.Modules.Merchandiser.Model.Classes
{
    public class ProductSampling : BaseModel, IProductSampling
    {
        public decimal SellingPrice { get; set; }
        public int UnitUsed { get; set; }
        public int UnitSold { get; set; }
        public int NoOfCustomerApproached { get; set; }
        public string BeatHistoryUID { get; set; }
        public string RouteUID { get; set; }
        public string JobPositionUID { get; set; }
        public string EmpUID { get; set; }
        public DateTime ExecutionTime { get; set; }
        public string StoreUID { get; set; }
        public string SKUUID { get; set; }
    }
} 