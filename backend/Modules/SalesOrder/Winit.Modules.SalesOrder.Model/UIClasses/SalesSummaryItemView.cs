using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Winit.Modules.SalesOrder.Model.UIInterfaces;

namespace Winit.Modules.SalesOrder.Model.UIClasses
{
    public class SalesSummaryItemView:ISalesSummaryItemView
    {
        public string SalesOrderUID { get; set; }
        public string StoreCode { get; set; }
        public string StoreName { get; set; }
        public string OrderNumber { get; set; }
        public string Address { get; set; }
        public string OrderType { get; set; }
        public string OrderStatus { get; set; }
        public DateTime OrderDate { get; set; }
        public DateTime DeliveryDate { get; set; }
        public decimal OrderAmount { get; set; }
        public string CurrencyLabel { get; set; }
        public bool IsPosted { get; set; }
        
    }
}
