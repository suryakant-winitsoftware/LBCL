using Winit.Modules.Invoice.Model.Interfaces;

namespace Winit.Modules.Invoice.Model.Classes;

public class InvoiceHeaderView : IInvoiceHeaderView
{
    public string UID { get; set; }
    public string? OrgUID { get; set; }
    public string? OrgCode { get; set; }
    public string? OrgName { get; set; }
    public string? OraclePONumber { get; set; }
    public string? WINITPONumber { get; set; }
    public string? ARNumber { get; set; }
    public string? InvoiceNo { get; set; }
    public string? GSTInvoiceNo { get; set; }
    public DateTime? InvoiceDate { get; set; }
    public string? InvoiceURL { get; set; }
    public decimal? TotalAmount { get; set; }
    public decimal? TotalTax { get; set; }
    public decimal? NetAmount { get; set; }
    public string POUID { get; set; }
}
