using Winit.Modules.Invoice.Model.Interfaces;
namespace Winit.Modules.Invoice.Model.Classes;

public class ProvisionComparisonView : IProvisionComparisonView
{
    public string ar_no { get; set; }
    public string gst_invoice_number { get; set; }
    public DateTime invoice_date { get; set; }
    public string item_code { get; set; }
    public Decimal qty { get; set; }
    public string provision_type { get; set; }
    public Decimal dms_Provision_Amount { get; set; }
    public Decimal oracle_Provision_Amount { get; set; }
    public Decimal Diff { get; set; }
}
