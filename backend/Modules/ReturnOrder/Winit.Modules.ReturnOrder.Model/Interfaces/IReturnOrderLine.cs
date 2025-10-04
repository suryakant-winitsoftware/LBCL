using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Winit.Modules.Base.Model;

namespace Winit.Modules.ReturnOrder.Model.Interfaces
{
    public interface IReturnOrderLine:IBaseModel
    {
        public string ReturnOrderUID { get; set; }
        public int LineNumber { get; set; }
        public string SKUUID { get; set; }
        public string SKUCode { get; set; }
        public string SKUType { get; set; }
        public decimal BasePrice { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal FakeUnitPrice { get; set; }
        public string BaseUOM { get; set; }
        public string UoM { get; set; }
        public decimal Multiplier { get; set; }
        public decimal Qty { get; set; }
        public decimal QtyBU { get; set; }
        public decimal ApprovedQty { get; set; }
        public decimal ReturnedQty { get; set; }
        public decimal TotalAmount { get; set; }
        public decimal TotalDiscount { get; set; }
        public decimal TotalExciseDuty { get; set; }
        public decimal TotalTax { get; set; }
        public decimal NetAmount { get; set; }
        public string SKUPriceUID { get; set; }
        public string SKUPriceListUID { get; set; }
        public string ReasonCode { get; set; }
        public string ReasonText { get; set; }
        public DateTime ExpiryDate { get; set; }
        public string BatchNumber { get; set; }
        public string SalesOrderUID { get; set; }
        public string SalesOrderLineUID { get; set; }
        public string Remarks { get; set; }
        public decimal Volume { get; set; }
        public string VolumeUnit { get; set; }
        public string PromotionUID { get; set; }
        public decimal NetFakeAmount { get; set; }
        public string PONumber { get; set; }
        public decimal AvailableQty { get; set; }
        public string TaxData { get; set; }
    }
}
