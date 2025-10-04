using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Winit.Modules.Base.Model;
using Winit.Modules.Scheme.Model.Interfaces;

namespace Winit.Modules.Scheme.Model.Classes
{
    public class StandingProvisionSchemeApplicableOrg: BaseModel,IStandingProvisionSchemeApplicableOrg
    {
        public string StandingProvisionSchemeUID { get; set; }
        public string OrgUID { get; set; }
    }
}
