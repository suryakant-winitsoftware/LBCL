using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Winit.Modules.Base.Model;
using Winit.Modules.Org.Model.Interfaces;

namespace Winit.Modules.Org.Model.Classes
{
    public class WarehouseStockItemView :BaseModel, IWarehouseStockItemView
    {
        public String UID { get; set; }
        public string SKUUID { get; set; }
        public string SKUCode { get; set; }
        public string SKUName { get; set; }
        public decimal Qty { get; set; }
        public string UOM { get; set; }
        public string StockType { get; set; }
        public decimal OuterMultiplier { get; set; }
        public int OuterQty { get; set; }
        public decimal EAQty { get; set; }
        public decimal CostPrice { get; set; }
        public decimal TotalCost { get; set; }
        public decimal TotalEAQty { get; set; }
        public decimal Net { get; set; }
        public string ReservedOUQty { get; set; }
        public string ReservedBUQty { get; set; }
        public string SKUImage { get; set; }
    }
}

