using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Winit.Modules.Base.Model;

namespace Winit.Modules.Org.Model.Interfaces
{
    public interface IWarehouseFOCStockItemView:IBaseModel
    {
        public String UID { get; set; }
        public string SKUCode { get; set; }
        public string SKUName { get; set; }      
        public string StockType { get; set; }
        public string Total { get; set; }
        public string Allocated { get; set; }
        public string Available { get; set; }     
        public int OuterQty { get; set; }
        public decimal EAQty { get; set; }
        public int OuterQty2 { get; set; }
        public decimal EAQty2 { get; set; }
        public int OuterQty3 { get; set; }
        public decimal EAQty3 { get; set; }

    }
}
