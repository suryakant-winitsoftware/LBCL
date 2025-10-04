namespace Winit.Modules.Scheme.Model.Interfaces;

public interface IStandingSchemePO
{
    string SKUUID { get; set; }
    string StandingProvisionUID { get; set; }
    string StandingProvisionCode { get; set; }
    decimal Amount { get; set; }
    string SkuCategoryCode { get; set; }
    string SkuTypeCode { get; set; }
    string StarRatingCode { get; set; }
    string SkuTonnageCode { get; set; }
    string StarCapacityCode { get; set; }
    string SkuSeriesCode { get; set; }
    string SkuProductGroup { get; set; }
    string ExcludedModelCommaSeparated { get; set; } // Comma-separated SKUs to exclude
    HashSet<string> ExcludedModels { get; set; }
    string DivisionOrgUIDsCommaSeparated { get; set; } // Comma-separated Division organization UIDs
    HashSet<string> DivisionOrgUIDs { get; set; } 
}
