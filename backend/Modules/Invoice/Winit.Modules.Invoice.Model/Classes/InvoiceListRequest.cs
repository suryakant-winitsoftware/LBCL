namespace Winit.Modules.Invoice.Model.Classes;

public class InvoiceListRequest
{
    public string? InvoiceNumber { get; set; }
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public string? ModelNameOrCode { get; set; }
    public string? OrgUID { get; set; }
    
}
