using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Winit.Modules.Base.Model;

namespace Winit.Modules.CollectionModule.Model.Interfaces;

public interface IPayment:IBaseModel
{
    string ReceiptNumber { get; set; }
    string ConsolidatedReceiptNumber { get; set; }
    string PaymentMode { get; set; }
    string Category { get; set; }
    string AccCollectionUID { get; set; }
    decimal Amount { get; set; }
    string CurrencyUID { get; set; }
    string DefaultCurrencyUID { get; set; }
    decimal DefaultCurrencyExchangeRate { get; set; }
    decimal DefaultCurrencyAmount { get; set; }
    string OrgUID { get; set; }
    string DistributionChannelUID { get; set; }
    string StoreUID { get; set; }
    string RouteUID { get; set; }
    string JobPositionUID { get; set; }
    string EmpUID { get; set; }
    DateTime? CollectedDate { get; set; }
    string Status { get; set; }
    string Remarks { get; set; }
    string ReferenceNumber { get; set; }
    bool IsRealized { get; set; }
    string Latitude { get; set; }
    string Longitude { get; set; }
    string Source { get; set; }
    bool IsMultimode { get; set; }
    DateTime? CancelledOn { get; set; }
    string CancelledBy { get; set; }
    string Comments { get; set; }
}
