using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Winit.Modules.PurchaseOrder.Model.Interfaces;

public interface IPurchaseOrderHeaderItem
{
    // UI Properties
    string UID { get; set; }
    string AsnNo { get; set; }
    string OrderNumber { get; set; }
    string WarehouseName { get; set; }
    DateTime OrderDate { get; set; }
    DateTime RequestedDeliveryDate { get; set; }
    DateTime CPEConfirmDateTime { get; set; }
    decimal NetAmount { get; set; }
    string Status { get; set; }
    string ERPStatus { get; set; }
    int SerialNumber { get; set; }

    // Properties for data retrieval
    string OrgUID { get; set; }
    string DivisionUID { get; set; }
    bool HasTemplate { get; set; }
    string? PurchaseOrderTemplateHeaderUID { get; set; }
    string WareHouseUID { get; set; }
    string ShippingAddressUID { get; set; }
    string BillingAddressUID { get; set; }
    string? App1EmpUID { get; set; }
    string? App2EmpUID { get; set; }
    string? App3EmpUID { get; set; }
    string? App4EmpUID { get; set; }
    string? App5EmpUID { get; set; }
    string? App6EmpUID { get; set; }
    string? DraftOrderNumber { get; set; }
    string ChannelPartnerCode { get; set; }
    string ChannelPartnerName { get; set; }
    string? OracleNo { get; set; }
    string? ReportingEmpName { get; set; }
    string? ReportingEmpCode { get; set; }
    public string? CreatedByEmpCode { get; set; }
    public string? CreatedByEmpName { get; set; }
    public string? OracleOrderStatus { get; set; }
    public DateTime? OracleOrderdate { get; set; }
}
