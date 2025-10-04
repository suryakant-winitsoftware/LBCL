using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Winit.Modules.Base.Model;
using Winit.Modules.ExchangeOrder.Model.Interfaces;

namespace Winit.Modules.ExchangeOrder.Model.Classes
{
    public class ExchangeOrderLine : BaseModel, IExchangeOrderLine
    {
        public string ExchangeOrderUID { get; set; }
        public int LineNumber { get; set; }
        public string TakeInSKUCode { get; set; }
        public string SKUType { get; set; }
        public string SKUSubType { get; set; }
        public string OrgUID { get; set; }
        public decimal BasePrice { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal FakeUnitPrice { get; set; }
        public decimal QuotationPrice { get; set; }
        public string UOM { get; set; }
        public string BaseUOM { get; set; }
        public decimal TakeInSKUQtyOu { get; set; }
        public decimal TakeInQtyBU { get; set; }
        public decimal TakeInTotalQtyBU { get; set; }
        public string GiveOutSKUCode { get; set; }
        public decimal GiveOutQtyOU { get; set; }
        public decimal GiveOutQtyBU { get; set; }
        public decimal GiveOutTotalQtyBU { get; set; }
        public string ReasonCode { get; set; }
        public string Reason { get; set; }
        public DateTime? ExpiryDate { get; set; }
        public string BatchNumber { get; set; }
        public string Remarks { get; set; }
    }

   
}
