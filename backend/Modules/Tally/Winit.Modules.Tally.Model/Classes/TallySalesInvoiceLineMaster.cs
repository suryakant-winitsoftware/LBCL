using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Winit.Modules.Base.Model;
using Winit.Modules.Tally.Model.Interfaces;

namespace Winit.Modules.Tally.Model.Classes
{
    public class TallySalesInvoiceLineMaster : BaseModel , ITallySalesInvoiceLineMaster
    {
        public string DmsUid { get; set; }
        public string Guid { get; set; }
        public string VoucherNumber { get; set; }
        public string StockItemName { get; set; }
        public string Rate { get; set; }
        public string Amount { get; set; }
        public string ActualQty { get; set; }
        public string BilledQty { get; set; }
        public decimal Gst { get; set; }
        public string DiscountPercentage { get; set; }
        public decimal Qty { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal TotalAmount { get; set; }
        public decimal TotalDiscount { get; set; }
        public decimal TotalTax { get; set; }
        public decimal NetAmount { get; set; }
    }
}
