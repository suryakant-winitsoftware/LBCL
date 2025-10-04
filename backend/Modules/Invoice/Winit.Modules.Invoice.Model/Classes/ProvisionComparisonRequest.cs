namespace Winit.Modules.Invoice.Model.Classes;

public class ProvisionComparisonRequest
{
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public string? ChannelPartner { get; set; }
    public string? BroadClassification { get; set; }
    public string? Branch { get; set; }
    public string? SalesOffice { get; set; }
    public string? ProvisionType { get; set; }
    public string? ShowDiffOnly { get; set; }

}
