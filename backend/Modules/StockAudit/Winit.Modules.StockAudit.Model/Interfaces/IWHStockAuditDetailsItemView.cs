using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Winit.Modules.Base.Model;
using Winit.Shared.Models.Enums;

namespace Winit.Modules.StockAudit.Model.Interfaces
{
    public interface IWHStockAuditDetailsItemView:IBaseModel
    {
        public int Id { get; set; }
        public string UID { get; set; }
        public string WHStockAuditUID { get; set; }
        public int LineNumber { get; set; }
        public string SKUUID { get; set; }
        public string SKUCode { get; set; }
        public string BatchNumber { get; set; }
        public DateTime ExpiryDate { get; set; }
        public string SerialNo { get; set; }
        public decimal BasePrice { get; set; }
        public decimal ExpectedQty { get; set; }
        public decimal ExpectedValue { get; set; }
        public string UOM1 { get; set; }
        public string UOM2 { get; set; }
        public string UOM { get; set; }
        public decimal Conversion1 { get; set; }
        public decimal Conversion2 { get; set; }
        public decimal? AvailableQty1 { get; set; }
        public decimal? AvailableQty2 { get; set; }
        public decimal? AvailableQty { get; set; }
        public decimal? AvailableValue { get; set; }
        public string StockType { get; set; }
        public decimal? DiscrepencyQty { get; set; }
        public decimal? DiscrepencyValue { get; set; }
        public decimal OpenBalanceBU { get; set; }
        public decimal StockReceiptQtyBU { get; set; }
        public decimal DeliveredQtyBU { get; set; }
        public decimal CreditsQtyBU { get; set; }
        public decimal CreditCageQtyBU { get; set; }
        public decimal CreditCustomerQtyBU { get; set; }
        public decimal AdjustmentQtyBU { get; set; }
        public decimal ClosingBalanceQtyBU { get; set; }
        public decimal TotalStockCountBU { get; set; }
        public decimal VarianceQtyBU { get; set; }
        public decimal VarianceValue { get; set; }
        public decimal FinalClosingQty { get; set; }
        public int SS { get; set; }
        public DateTime CreatedTime { get; set; }
        public DateTime ModifiedTime { get; set; }
        public DateTime ServerAddTime { get; set; }
        public DateTime ServerModifiedTime { get; set; }
        public decimal TotalStockCountValue { get; set; }
        public DateTime LastReconciliationTime { get; set; }
        public ActionType ActionType { get; set; }
    }
}
