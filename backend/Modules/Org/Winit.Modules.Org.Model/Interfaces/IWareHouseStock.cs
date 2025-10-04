using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Winit.Modules.Org.Model.Interfaces
{
    public interface IWareHouseStock
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
