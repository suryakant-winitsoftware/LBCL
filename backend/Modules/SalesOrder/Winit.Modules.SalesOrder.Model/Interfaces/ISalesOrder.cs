using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Winit.Modules.Base.Model;
using Winit.Shared.Models.Enums;

namespace Winit.Modules.SalesOrder.Model.Interfaces;

public interface ISalesOrder : IBaseModelV2
{
    public string SalesOrderNumber { get; set; }
    public string DraftOrderNumber { get; set; }
    public string? CompanyUID { get; set; }
    public string OrgUID { get; set; }
    public string DistributionChannelUID { get; set; }
    public string DeliveredByOrgUID { get; set; }
    public string StoreUID { get; set; }
    public string Status { get; set; }
    public string OrderType { get; set; }
    public DateTime OrderDate { get; set; }
    public DateTime ExpectedDeliveryDate { get; set; }
    public DateTime DeliveredDateTime { get; set; }
    public string CustomerPO { get; set; }
    public string CurrencyUID { get; set; }
    public string PaymentType { get; set; }
    public decimal TotalAmount { get; set; }
    public decimal TotalDiscount { get; set; }
    public decimal TotalTax { get; set; }
    public decimal NetAmount { get; set; }
    public int LineCount { get; set; }
    public int QtyCount { get; set; }
    public decimal TotalFakeAmount { get; set; }
    public string ReferenceNumber { get; set; }
    public string Source { get; set; }
    public decimal TotalLineDiscount { get; set; }
    public decimal TotalCashDiscount { get; set; }
    public decimal TotalHeaderDiscount { get; set; }
    public decimal TotalExciseDuty { get; set; }
    public decimal TotalLineTax { get; set; }
    public decimal TotalHeaderTax { get; set; }
    public string CashSalesCustomer { get; set; }
    public string CashSalesAddress { get; set; }
    public string ReferenceUID { get; set; }
    public string ReferenceType { get; set; }
    public string JobPositionUID { get; set; }
    public string EmpUID { get; set; }
    public string BeatHistoryUID { get; set; }
    public string RouteUID { get; set; }
    public string StoreHistoryUID { get; set; }
    public decimal TotalCreditLimit { get; set; }
    public decimal AvailableCreditLimit { get; set; }
    public string Latitude { get; set; }
    public string Longitude { get; set; }
    public bool IsOffline { get; set; }
    public int CreditDays { get; set; }
    public string Notes { get; set; }
    public string DeliveryInstructions { get; set; }
    public string Remarks { get; set; }
    public bool IsTemperatureCheckEnabled { get; set; }
    public int AlwaysPrintedFlag { get; set; }
    public int PurchaseOrderNoRequiredFlag { get; set; }
    public bool IsWithPrintedInvoicesFlag { get; set; }
    public string TaxData { get; set; }
    public string DefaultBatchNumber { get; set; }
    public string DefaultStockVersion { get; set; }
    public string VehicleUID { get; set; }
    public bool IsStockUpdateRequired { get; set; }
    public bool IsInvoiceGenerationRequired { get; set; }
    public ActionType ActionType { get; set; }

}
