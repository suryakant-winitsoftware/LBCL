using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Winit.Modules.Base.Model;
using Winit.Shared.Models.Enums;

namespace Winit.Modules.SKUClass.Model.Interfaces
{
    public interface ISKUClassGroup:IBaseModel
    {
        string CompanyUID { get; set; }
        string SKUClassUID { get; set; }
        string Name { get; set; }
        string Description { get; set; }
        string OrgUID { get; set; }
        string DistributionChannelUID { get; set; }
        string FranchiseeOrgUID { get; set; }
        bool IsActive { get; set; }
        DateTime FromDate { get; set; }
        DateTime ToDate { get; set; }
        string SourceType { get; set; }
        DateTime? SourceDate { get; set; }
        int Priority { get; set; }
        ActionType ActionType { get; set; }
    }
}
