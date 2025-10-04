using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Winit.Modules.ETL.Invoice.Model.Interfaces
{
    public interface IStgInvoiceLine
    {
        public long Id { get; set; }
        public string InvoiceLineUid { get; set; }
        public string InvoiceUid { get; set; }
        public string InvoiceNumber { get; set; }
        public string? ProformaInvoiceNumber { get; set; }
        public string? DmsPurchaseOrderNumber { get; set; }
        public string? ErpOrderNumber { get; set; }
        public string? WarehouseUid { get; set; }
        public string? WarehouseCode { get; set; }
        public string? WarehouseName { get; set; }
        public string? OrgUid { get; set; }
        public string? OrgCode { get; set; }
        public string? OrgName { get; set; }
        public string? OrgUnitUid { get; set; }
        public string? OrgUnitCode { get; set; }
        public string? OrgUnitName { get; set; }
        public string? StoreUid { get; set; }
        public string? StoreCode { get; set; }
        public string? StoreName { get; set; }
        public string? BranchUid { get; set; }
        public string? BranchCode { get; set; }
        public string? BranchName { get; set; }
        public string? AsmEmpUid { get; set; }
        public string? AsmEmpCode { get; set; }
        public string? AsmEmpName { get; set; }
        public DateTime? DmsOrderDate { get; set; }
        public DateTime? DmsExpectedDeliveryDate { get; set; }
        public DateTime InvoiceDate { get; set; }
        public DateTime? DeliveredDateTime { get; set; }
        public string? CustomerPo { get; set; }
        public string? ArNumber { get; set; }
        public string? FiscalYear { get; set; }
        public int? PeriodYear { get; set; }
        public int? PeriodNum { get; set; }
        public int? QuarterNum { get; set; }
        public int LineNumber { get; set; }
        public string SkuUid { get; set; }
        public string SkuCode { get; set; }
        public string SkuName { get; set; }
        public string? SkuType { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal Qty { get; set; }
        public string? Uom { get; set; }
        public string? BaseUom { get; set; }
        public decimal? UomMultiplier { get; set; }
        public decimal TotalAmount { get; set; }
        public decimal TotalDiscount { get; set; }
        public decimal TotalTax { get; set; }
        public decimal NetAmount { get; set; }
        public decimal? Volume { get; set; }
        public string? VolumeUnit { get; set; }
        public decimal? Weight { get; set; }
        public string? WeightUnit { get; set; }
        public DateTime? CreatedTime { get; set; }
        public DateTime? ModifiedTime { get; set; }
        public string? Attribute1Code { get; set; }
        public string? Attribute1Name { get; set; }
        public string? Attribute2Code { get; set; }
        public string? Attribute2Name { get; set; }
        public string? Attribute3Code { get; set; }
        public string? Attribute3Name { get; set; }
        public string? Attribute4Code { get; set; }
        public string? Attribute4Name { get; set; }
        public string? Attribute5Code { get; set; }
        public string? Attribute5Name { get; set; }
        public string? Attribute6Code { get; set; }
        public string? Attribute6Name { get; set; }
        public string? Attribute7Code { get; set; }
        public string? Attribute7Name { get; set; }
    }
}
