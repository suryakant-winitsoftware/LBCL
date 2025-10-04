using Winit.Modules.Base.Model;
using Winit.Modules.CollectionModule.Model.Interfaces;

namespace Winit.Modules.CollectionModule.Model.Classes;

public class Payment : BaseModel, IPayment
{
    public string ReceiptNumber { get; set; }
    public string ConsolidatedReceiptNumber { get; set; }
    public string PaymentMode { get; set; }
    public string Category { get; set; }
    public string AccCollectionUID { get; set; }
    public decimal Amount { get; set; }
    public string CurrencyUID { get; set; }
    public string DefaultCurrencyUID { get; set; }
    public decimal DefaultCurrencyExchangeRate { get; set; }
    public decimal DefaultCurrencyAmount { get; set; }
    public string OrgUID { get; set; }
    public string DistributionChannelUID { get; set; }
    public string StoreUID { get; set; }
    public string RouteUID { get; set; }
    public string JobPositionUID { get; set; }
    public string EmpUID { get; set; }
    public DateTime? CollectedDate { get; set; }
    public string Status { get; set; }
    public string Remarks { get; set; }
    public string ReferenceNumber { get; set; }
    public bool IsRealized { get; set; }
    public string Latitude { get; set; }
    public string Longitude { get; set; }
    public string Source { get; set; }
    public bool IsMultimode { get; set; }
    public DateTime? CancelledOn { get; set; }
    public string CancelledBy { get; set; }
    public string Comments { get; set; }
}
