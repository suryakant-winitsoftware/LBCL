using Winit.Modules.Base.Model;
using Winit.Modules.Common.Model.Classes.AuditTrail;
using Winit.Modules.PurchaseOrder.Model.Interfaces;

namespace Winit.Modules.PurchaseOrder.Model.Classes;

public class PurchaseOrderHeader : BaseModel, IPurchaseOrderHeader
{
    public string OrgUID { get; set; } = string.Empty;
    public string? DivisionUID { get; set; }
    [AuditTrail]
    public bool HasTemplate { get; set; }
    [AuditTrail]
    public string? PurchaseOrderTemplateHeaderUID { get; set; }
    public string WareHouseUID { get; set; } = string.Empty;
    [AuditTrail("DMS Creation Date")]
    public DateTime OrderDate { get; set; }
    public string? OrderNumber { get; set; }
    public string? DraftOrderNumber { get; set; }
    [AuditTrail("DMS Purchase Order No")]
    public string? DMSOrderNumber { get { return DraftOrderNumber ?? OrderNumber ?? "NA"; } }
    [AuditTrail]
    public DateTime ExpectedDeliveryDate { get; set; }
    [AuditTrail]
    public string ShippingAddressUID { get; set; } = string.Empty;
    [AuditTrail]
    public string BillingAddressUID { get; set; } = string.Empty;
    [AuditTrail]
    public string Status { get; set; } = string.Empty;
    [AuditTrail]
    public decimal QtyCount { get; set; }
    [AuditTrail]
    public int LineCount { get; set; }
    public decimal TotalAmount { get; set; }
    [AuditTrail]
    public decimal TotalDiscount { get; set; }
    public decimal LineDiscount { get; set; }
    public decimal HeaderDiscount { get; set; }
    public decimal TotalTaxAmount { get; set; }
    public decimal LineTaxAmount { get; set; }
    public decimal HeaderTaxAmount { get; set; }
    [AuditTrail("Order Value")]
    public decimal NetAmount { get; set; }
    public decimal AvailableCreditLimit { get; set; }
    public string TaxData { get; set; } = string.Empty;
    public string? App1EmpUID { get; set; }
    public string? App2EmpUID { get; set; }
    public string? App3EmpUID { get; set; }
    public string? App4EmpUID { get; set; }
    public string? App5EmpUID { get; set; }
    public string? App6EmpUID { get; set; }
    public DateTime? App1Date { get; set; }
    public DateTime? App2Date { get; set; }
    public DateTime? App3Date { get; set; }
    public DateTime? App4Date { get; set; }
    public DateTime? App5Date { get; set; }
    public DateTime? App6Date { get; set; }
    [AuditTrail]
    public string BranchUID { get; set; } = string.Empty;
    public string HOOrgUID { get; set; } = string.Empty;
    public string OrgUnitUID { get; set; } = string.Empty;
    public string? ReportingEmpUID { get; set; }
    public string? SourceWareHouseUID { get; set; }
    public string? ReportingEmpName { get; set; }
    public string? ReportingEmpCode { get; set; }
    [AuditTrail]
    public string? CreatedByEmpCode { get; set; }
    [AuditTrail]
    public string? CreatedByEmpName { get; set; }
    public decimal TotalBilledQty { get; set; }
    public decimal TotalCancelledQty { get; set; }
    public string? OracleOrderStatus { get; set; }
    public bool IsApprovalCreated { get; set; }
    public string? OrgName { get; set; }
    public string? OrgCode { get; set; }
    public string? WarehouseName { get; set; }
}
