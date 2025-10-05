using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Winit.Modules.PurchaseOrder.Model.Interfaces;

namespace Winit.Modules.PurchaseOrder.Model.Classes;

public class PurchaseOrderHeaderItem : IPurchaseOrderHeaderItem
{
    // UI Properties
    public string UID { get; set; } = string.Empty;
    public string AsnNo { get; set; }
    public string WarehouseName { get; set; }
    public DateTime OrderDate { get; set; }
    public DateTime RequestedDeliveryDate { get; set; }
    public DateTime CPEConfirmDateTime { get; set; }
    public decimal NetAmount { get; set; }
    public decimal QtyCount { get; set; }
    public int LineCount { get; set; }
    public decimal TotalAmount { get; set; }
    public string Status { get; set; }
    public string ERPStatus { get; set; }
    public int SerialNumber { get; set; }

    // Properties for data retrieval
    public string OrgUID { get; set; }
    public string DivisionUID { get; set; }
    public bool HasTemplate { get; set; }
    public string? PurchaseOrderTemplateHeaderUID { get; set; }
    public string WareHouseUID { get; set; }
    public string ShippingAddressUID { get; set; }
    public string BillingAddressUID { get; set; }
    public string? App1EmpUID { get; set; }
    public string? App2EmpUID { get; set; }
    public string? App3EmpUID { get; set; }
    public string? App4EmpUID { get; set; }
    public string? App5EmpUID { get; set; }
    public string? App6EmpUID { get; set; }
    public string OrderNumber { get; set; } = string.Empty;
    public string? DraftOrderNumber { get; set; }
    public string ChannelPartnerCode { get; set; } = string.Empty;
    public string ChannelPartnerName { get; set; } = string.Empty;
    public string? OracleNo { get; set; }
    public string? ReportingEmpName { get; set; }
    public string? ReportingEmpCode { get; set; }
    public string? CreatedByEmpCode { get; set; }
    public string? CreatedByEmpName { get; set; }
    public string? OracleOrderStatus { get; set; }
    public DateTime? OracleOrderdate { get; set; }
}
