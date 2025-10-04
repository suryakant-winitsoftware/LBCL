using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Winit.Modules.SalesOrder.Model.Interfaces;

namespace Winit.Modules.SalesOrder.Model.Classes
{
    public class SalesOrderLinePrintView: ISalesOrderLinePrintView
    {
        public int LineNumber { get; set; }
        public string SKUCode { get; set; }
        public string SKUDescription { get; set; }
        public string ItemType { get; set; }
        public decimal UnitPrice { get; set; }
        public string UoM { get; set; }
        public decimal UOMConversionToBU { get; set; }
        public string RecoUOM { get; set; }
        public decimal RecoQty { get; set; }
        public decimal Qty { get; set; }
        public decimal DeliveredQty { get; set; }
        public decimal TotalAmount { get; set; }
        public decimal TotalDiscount { get; set; }
        public decimal TotalTax { get; set; }
        public decimal NetAmount { get; set; }
    }
}
