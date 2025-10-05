using Winit.Modules.Base.Model;

namespace Winit.Modules.PurchaseOrder.Model.Interfaces;

public interface IPurchaseOrderHeader : IBaseModel
{
    public string OrgUID { get; set; }
    public string? DivisionUID { get; set; }
    public bool HasTemplate { get; set; }
    public string? PurchaseOrderTemplateHeaderUID { get; set; }
    public string WareHouseUID { get; set; }
    public DateTime OrderDate { get; set; }
    public string? OrderNumber { get; set; }
    public string? DraftOrderNumber { get; set; }
    public string? DMSOrderNumber { get; }
    public DateTime ExpectedDeliveryDate { get; set; }
    public string ShippingAddressUID { get; set; }
    public string BillingAddressUID { get; set; }
    public string Status { get; set; }
    public decimal QtyCount { get; set; }
    public int LineCount { get; set; }
    public decimal TotalAmount { get; set; }
    public decimal TotalDiscount { get; set; }
    public decimal LineDiscount { get; set; }
    public decimal HeaderDiscount { get; set; }
    public decimal TotalTaxAmount { get; set; }
    public decimal LineTaxAmount { get; set; }
    public decimal HeaderTaxAmount { get; set; }
    public decimal NetAmount { get; set; }
    public decimal AvailableCreditLimit { get; set; }
    public string TaxData { get; set; }
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
    public string BranchUID { get; set; }
    public string HOOrgUID { get; set; }
    public string OrgUnitUID { get; set; }
    public string? ReportingEmpUID { get; set; }
    public string? SourceWareHouseUID { get; set; }
    public string? ReportingEmpName { get; set; }
    public string? ReportingEmpCode { get; set; }
    public string? CreatedByEmpCode { get; set; }
    public string? CreatedByEmpName { get; set; }
    public decimal TotalBilledQty { get; set; }
    public decimal TotalCancelledQty { get; set; }
    public string? OracleOrderStatus { get; set; }
    public bool IsApprovalCreated { get; set; }
    public string? OrgName { get; set; }
    public string? OrgCode { get; set; }
    public string? WarehouseName { get; set; }
}
