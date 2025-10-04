using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Winit.Modules.ApprovalEngine.Model.Classes;
using Winit.Modules.Scheme.Model.Interfaces;

namespace Winit.Modules.Scheme.Model.Interfaces
{
    public interface IStandingProvisionSchemeMaster
    {
        bool IsNew { get; set; }
        bool IsFinalApproval { get; set; }
        IStandingProvisionScheme StandingProvisionScheme { get; set; }
        List<ISchemeBranch> SchemeBranches { get; set; }
        List<ISchemeBroadClassification> SchemeBroadClassifications { get; set; }
        List<ISchemeOrg> SchemeOrgs { get; set; }
        List<IStandingProvisionSchemeDivision> StandingProvisionSchemeDivisions { get; set; }
        ApprovalRequestItem? ApprovalRequestItem { get; set; }
        ApprovalStatusUpdate? ApprovalStatusUpdate { get; set; }

    }
}
