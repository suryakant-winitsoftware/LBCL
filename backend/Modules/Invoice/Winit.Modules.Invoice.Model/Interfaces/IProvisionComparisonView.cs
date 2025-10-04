namespace Winit.Modules.Invoice.Model.Interfaces;

public interface IProvisionComparisonView
{
    string ar_no { get; set; }
    string gst_invoice_number { get; set; }
    DateTime invoice_date { get; set; }
    string item_code { get; set; }
    Decimal qty { get; set; }
    string provision_type { get; set; }
    Decimal dms_Provision_Amount { get; set; }
    Decimal oracle_Provision_Amount { get; set; }
    Decimal Diff { get; set; }
}
