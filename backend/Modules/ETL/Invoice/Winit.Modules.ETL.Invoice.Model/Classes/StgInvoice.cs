using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Winit.Modules.ETL.Invoice.Model.Interfaces;

namespace Winit.Modules.ETL.Invoice.Model.Classes
{
    public class StgInvoice : IStgInvoice
    {
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
        public string? ShippingAddressCode { get; set; }
        public string? BillingAddressCode { get; set; }
        public decimal TotalAmount { get; set; }
        public decimal TotalDiscount { get; set; }
        public decimal TotalTax { get; set; }
        public decimal NetAmount { get; set; }
        public int LineCount { get; set; }
        public int QtyCount { get; set; }
        public string? FiscalYear { get; set; }
        public int? PeriodYear { get; set; }
        public int? PeriodNum { get; set; }
        public int? QuarterNum { get; set; }
        public DateTime? CreatedTime { get; set; }
        public DateTime? ModifiedTime { get; set; }

        public List<IStgInvoiceLine> InvoiceLines { get; set; } = new List<IStgInvoiceLine>();
    }
}
