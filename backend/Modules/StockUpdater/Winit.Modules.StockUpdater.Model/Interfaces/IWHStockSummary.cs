using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Winit.Modules.Base.Model;
using Winit.Shared.Models.Enums;

namespace Winit.Modules.StockUpdater.Model.Interfaces
{
    public interface IWHStockSummary 
    {

        public string OrgUID { get; set; }
        public string WarehouseUID { get; set; }
        public string WarehouseCode { get; set; }
        public string WarehouseName { get; set; }
        public string SKUUID { get; set; }
        public string SKUCode { get; set; }
        public string SKUName { get; set; }
        public string BatchNumber { get; set; }
        public decimal SalableQty { get; set; }
        public decimal NonSalableQty { get; set; }
        public decimal ReservedQty { get; set; }
        public decimal TotalQty { get; set; }
    }
}
