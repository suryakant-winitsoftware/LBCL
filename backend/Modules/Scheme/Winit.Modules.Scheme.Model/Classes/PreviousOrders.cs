using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Winit.Modules.Scheme.Model.Interfaces;

namespace Winit.Modules.Scheme.Model.Classes
{
    public class PreviousOrders : IPreviousOrders
    {
        public string OrderUID { get; set; }  // Purchase order header UID
        public string OrgUID { get; set; }  // Organization UID
        public string SKUUID { get; set; }  
        public string SKUCode { get; set; } 
        public string SKUName { get; set; }  
        public decimal UnitPrice { get; set; }  // Unit price for the current purchase order
        public DateTime ModifiedTime { get; set; }  // Last modified time of the purchase order
        public decimal LastUnitPrice { get; set; }  // Last unit price of the SKU from previous orders
    }

}
