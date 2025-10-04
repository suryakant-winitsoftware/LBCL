using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Winit.Modules.SalesOrder.Model.UIInterfaces;

namespace Winit.Modules.SalesOrder.Model.Event
{
    public class SalesOrderItemEvent
    {
        public ISalesOrderItemView SalesOrderItemView { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal Qty { get; set; }
    }
}
