using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Winit.Modules.Base.Model;

namespace Winit.Modules.Scheme.Model.Interfaces
{
    public interface IStandingProvisionSchemeBranch :IBaseModel
    {
        string StandingProvisionSchemeUID { get; set; }
        string BranchCode { get; set; }
        string CreatedByEmpName { get; set; }

    }
}
