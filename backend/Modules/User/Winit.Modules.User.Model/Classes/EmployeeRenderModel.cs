using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Winit.Modules.User.Model.Classes
{
    public class EmployeeRenderModel
    {
        public bool IsEmployeeInformationRendered { get; set; } = true;
        public bool IsOrg_RoleMapinngInformationRendered { get; set; } 
        public bool IsApplicableOrganizationInformationRendered { get; set; }

        public bool IsLocationMappingInformationRendered { get; set; }


    }
}
