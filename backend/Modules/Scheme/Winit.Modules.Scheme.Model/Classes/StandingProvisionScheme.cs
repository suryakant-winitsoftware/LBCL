using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Winit.Modules.Base.Model;
using Winit.Modules.Scheme.Model.Interfaces;

namespace Winit.Modules.Scheme.Model.Classes
{
    public class StandingProvisionScheme : BaseModel, IStandingProvisionScheme
    {
        public string JobPositionUID { get; set; }
        public string Code { get; set; }
        public string CreatedByEmpName { get; set; }
        public string OrgUID { get; set; }
        public string SKUCategoryCode { get; set; }
        public string SKUCategoryName { get; set; }
        public string SKUTypeCode { get; set; }
        public string SKUTypeName { get; set; }
        public string StarRatingCode { get; set; }
        public string StarRatingName { get; set; }
        public string SKUTonnageCode { get; set; }
        public string SKUTonnageName { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public decimal Amount { get; set; }
        public string Description { get; set; }
        public string ExcludedModels { get; set; }
        public string Branch { get; set; }
        public string StarCapacityCode { get; set; }
        public string StarCapacityName { get; set; }
        public string SKUSeriesCode { get; set; }
        public string SKUSeriesName { get; set; }
        public string SKUProductGroup { get; set; }
        public string Status { get; set; }
        public bool IsApprovalCreated { get; set; }

        public string EndDateRemarks { get; set; }
        public string EndDateUpdatedByEmpUID { get; set; }
        public DateTime EndDateUpdatedOn { get; set; }
        public bool HasHistory { get; set; }
        public ISchemeExtendHistory SchemeExtendHistory { get; set; }
    }
}
