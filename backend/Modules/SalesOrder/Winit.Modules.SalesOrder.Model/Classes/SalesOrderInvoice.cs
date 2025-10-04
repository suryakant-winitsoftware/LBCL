using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Winit.Modules.SalesOrder.Model.Interfaces;

namespace Winit.Modules.SalesOrder.Model.Classes
{
    public class SalesOrderInvoice:ISalesOrderInvoice
    {
        public string SalesOrderUID { get; set; } = string.Empty;
        public string SalesOrderNumber { get; set; } = string.Empty;
        public DateTime DeliveryDate { get; set; }
        public int TotalQuantity { get; set; }
        public int AvailableQty { get; set; }
    }
}
