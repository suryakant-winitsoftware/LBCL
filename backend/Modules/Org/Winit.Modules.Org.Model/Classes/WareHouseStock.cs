using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Winit.Modules.Org.Model.Interfaces;

namespace Winit.Modules.Org.Model.Classes
{
    public class WareHouseStock : IWareHouseStock
    {
        public string OUCode { get; set; }
        public string WarehouseCode { get; set; }
        public string SubWarehouseCode { get; set; }
        public string SKUCode { get; set; }
        public string SKUName { get; set; }
        public int Quantity { get; set; }
        public string WarehouseName { get; set; }
        public string SubWarehouseName { get; set; }
    }
}
