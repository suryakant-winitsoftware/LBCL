using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Winit.Modules.DashBoard.Model.Interfaces
{
    public interface IInvoiceLine
    {
        string InvoiceLineUid { get; set; }
        string InvoiceUid { get; set; }
        string InvoiceNumber { get; set; }
        string ProformaInvoiceNumber { get; set; }
        string DmsPurchaseOrderNumber { get; set; }
        string ErpOrderNumber { get; set; }
        string WarehouseUid { get; set; }
        string WarehouseCode { get; set; }
        string WarehouseName { get; set; }
        string OrgUid { get; set; }
        string OrgCode { get; set; }
        string OrgName { get; set; }
        string OrgUnitUid { get; set; }
        string OrgUnitCode { get; set; }
        string OrgUnitName { get; set; }
        string StoreUid { get; set; }
        string StoreCode { get; set; }
        string StoreName { get; set; }
        string BranchUid { get; set; }
        string BranchCode { get; set; }
        string BranchName { get; set; }
        string AsmEmpUid { get; set; }
        string AsmEmpCode { get; set; }
        string AsmEmpName { get; set; }
        DateTime? DmsOrderDate { get; set; }
        DateTime? DmsExpectedDeliveryDate { get; set; }
        DateTime? InvoiceDate { get; set; }
        DateTime? DeliveredDateTime { get; set; }
        string CustomerPo { get; set; }
        string ArNumber { get; set; }
        string FiscalYear { get; set; }
        int? PeriodYear { get; set; }
        int? PeriodNum { get; set; }
        int? QuarterNum { get; set; }
        int LineNumber { get; set; }
        string SkuUid { get; set; }
        string SkuCode { get; set; }
        string SkuName { get; set; }
        string SkuType { get; set; }
        decimal UnitPrice { get; set; }
        int Qty { get; set; }
        string Uom { get; set; }
        string BaseUom { get; set; }
        decimal UomMultiplier { get; set; }
        decimal TotalAmount { get; set; }
        decimal TotalDiscount { get; set; }
        decimal TotalTax { get; set; }
        decimal NetAmount { get; set; }
        decimal Volume { get; set; }
        string VolumeUnit { get; set; }
        decimal Weight { get; set; }
        string WeightUnit { get; set; }
        DateTime CreatedTime { get; set; }
        DateTime ModifiedTime { get; set; }
        string Attribute1Code { get; set; }
        string Attribute1Name { get; set; }
        string Attribute2Code { get; set; }
        string Attribute2Name { get; set; }
        string Attribute3Code { get; set; }
        string Attribute3Name { get; set; }
        string Attribute4Code { get; set; }
        string Attribute4Name { get; set; }
        string Attribute5Code { get; set; }
        string Attribute5Name { get; set; }
        string Attribute6Code { get; set; }
        string Attribute6Name { get; set; }
        string Attribute7Code { get; set; }
        string Attribute7Name { get; set; }

    }
}
