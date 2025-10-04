using Winit.Modules.Scheme.Model.Interfaces;
namespace Winit.Modules.Scheme.Model.Classes;

public class StandingSchemePO : IStandingSchemePO
{
    public string SKUUID { get; set; }
    public string StandingProvisionUID { get; set; }
    public string StandingProvisionCode { get; set; }
    public decimal Amount { get; set; }
    public string SkuCategoryCode { get; set; }
    public string SkuTypeCode { get; set; }
    public string StarRatingCode { get; set; }
    public string SkuTonnageCode { get; set; }
    public string StarCapacityCode { get; set; }
    public string SkuSeriesCode { get; set; }
    public string SkuProductGroup { get; set; }
    public string ExcludedModelCommaSeparated { get; set; } // Comma-separated SKUs to exclude
    public HashSet<string> ExcludedModels { get; set; }
    public string DivisionOrgUIDsCommaSeparated { get; set; } // Comma-separated Division organization UIDs
    public HashSet<string> DivisionOrgUIDs { get; set; }
}
