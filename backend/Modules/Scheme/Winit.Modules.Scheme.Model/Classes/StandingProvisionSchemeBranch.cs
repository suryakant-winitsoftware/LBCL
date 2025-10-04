using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Winit.Modules.Base.Model;
using Winit.Modules.Scheme.Model.Interfaces;

namespace Winit.Modules.Scheme.Model.Classes
{
    public class StandingProvisionSchemeBranch : BaseModel, IStandingProvisionSchemeBranch
    {
       public string StandingProvisionSchemeUID { get; set; }
       public string BranchCode { get; set; }
       public string CreatedByEmpName { get; set; }

    }
}
