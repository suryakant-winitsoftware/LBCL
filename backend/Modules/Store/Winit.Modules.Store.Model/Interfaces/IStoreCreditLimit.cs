
namespace Winit.Modules.Store.Model.Interfaces;

public interface IStoreCreditLimit
{
    string Division { get; set; }
    string StoreUID { get; set; }
    decimal CreditLimit { get; set; }
    decimal TemporaryCreditLimit { get; set; }
    decimal CurrentOutstanding { get; set; }
    int CreditDays { get; set; }
    int MaxAgingDays { get; set; }
    int TemporaryCreditDays { get; set; }
    DateTime? DueDate { get; set; }
    DateTime? TemporaryCreditApprovalDate { get; set; }
    decimal BlockedLimit { get; set; }
}
