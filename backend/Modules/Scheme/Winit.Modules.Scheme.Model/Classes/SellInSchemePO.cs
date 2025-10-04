using Winit.Modules.Scheme.Model.Interfaces;
namespace Winit.Modules.Scheme.Model.Classes;

public class SellInSchemePO : ISellInSchemePO
{
    public string SchemeUID { get; set; }
    public string SchemeCode { get; set; }
    public string SKUUID { get; set; }
    public decimal InvoiceDiscount { get; set; }
    public decimal CnP1 { get; set; }
    public decimal CnP2 { get; set; }
    public decimal CnP3 { get; set; }
    public string InvoiceDiscountType { get; set; }
    public string CreditNoteDiscountType { get; set; }
    public string SellInSchemeLineUID { get; set; }
    public string EndedBySellInSchemeLineUID { get; set; }
}
