using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Winit.Modules.Base.Model;

namespace Winit.Modules.Scheme.Model.Interfaces
{
    public interface IStandingProvisionSchemeApplicableOrg:IBaseModel
    {
        string StandingProvisionSchemeUID { get; set; }
        string OrgUID { get; set; }
    }
}
