using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Winit.Modules.Base.Model;

namespace Winit.Modules.Scheme.Model.Interfaces
{
    public interface IStandingProvisionScheme : IBaseModel
    {
        string OrgUID { get; set; }
        string Code { get; set; }
        string CreatedByEmpName { get; set; }
        string JobPositionUID { get; set; }
        string SKUCategoryCode { get; set; }
        string SKUCategoryName { get; set; }
        string SKUTypeCode { get; set; }
        string SKUTypeName { get; set; }
        string StarRatingName { get; set; }
        string StarRatingCode { get; set; }
        string SKUTonnageCode { get; set; }
        string SKUTonnageName { get; set; }

        DateTime? StartDate { get; set; }
        DateTime? EndDate { get; set; }
        decimal Amount { get; set; }
        string Description { get; set; }
        string ExcludedModels { get; set; }
        string Branch { get; set; }
        string StarCapacityCode { get; set; }
        string StarCapacityName { get; set; }
        string SKUSeriesCode { get; set; }
        string SKUSeriesName { get; set; }
        string SKUProductGroup { get; set; }
        string Status { get; set; }
        bool IsApprovalCreated { get; set; }
        string EndDateRemarks { get; set; }
        string EndDateUpdatedByEmpUID { get; set; }
        DateTime EndDateUpdatedOn { get; set; }
        bool HasHistory {  get; set; }
        ISchemeExtendHistory SchemeExtendHistory { get; set; }
    }
}
