using Winit.Modules.Store.Model.Interfaces;

namespace Winit.Modules.Store.Model.Classes;

public class StoreCreditLimit : IStoreCreditLimit
{
    public string Division { get; set; }
    public required string StoreUID { get; set; }
    public decimal CreditLimit { get; set; }
    public decimal TemporaryCreditLimit { get; set; }
    public decimal CurrentOutstanding { get; set; }
    public int CreditDays { get; set; }
    public int MaxAgingDays { get; set; }
    public int TemporaryCreditDays { get; set; }
    public DateTime? DueDate { get; set; }
    public DateTime? TemporaryCreditApprovalDate { get; set; }
    public decimal BlockedLimit { get; set; }
}
