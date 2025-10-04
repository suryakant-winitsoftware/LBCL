using Winit.Modules.Invoice.Model.Interfaces;

namespace Winit.Modules.Invoice.Model.Classes;

public class InvoiceApprove : IInvoiceApprove
{
    public string UID { get; set; }
    public string ChannelPartnerCode { get; set; }
    public string ChannelPartnerName { get; set; }
    public string InvoiceNumber { get; set; }
    public DateTime InvoiceDate { get; set; }
    public decimal CreditNoteAmount1 { get; set; }
    public decimal CreditNoteAmount2 { get; set; }
    public decimal CreditNoteAmount3 { get; set; }
    public decimal CreditNoteAmount4 { get; set; }
    public string LinkedItemType { get; set; }
    public string LinkedItemUid { get; set; }
}
