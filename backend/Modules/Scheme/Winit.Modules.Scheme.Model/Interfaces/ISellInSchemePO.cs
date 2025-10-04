namespace Winit.Modules.Scheme.Model.Interfaces;

public interface ISellInSchemePO
{
    string SchemeUID { get; set; }
    string SKUUID { get; set; }
    string SchemeCode { get; set; }
    decimal InvoiceDiscount { get; set; }
    decimal CnP1 { get; set; }
    decimal CnP2 { get; set; }
    decimal CnP3 { get; set; }
    string InvoiceDiscountType { get; set; }
    string CreditNoteDiscountType { get; set; }
    string SellInSchemeLineUID { get; set; }
    string EndedBySellInSchemeLineUID { get; set; }
}
