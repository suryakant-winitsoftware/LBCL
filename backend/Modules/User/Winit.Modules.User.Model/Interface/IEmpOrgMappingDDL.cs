using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Winit.Modules.User.Model.Interfaces
{
    public interface IEmpOrgMappingDDL
    {
        public string EmpOrgMappingUID { get; set; }
        public string EmpUID { get; set; }
        public string OrgUID { get; set; }
        public string OrgCode { get; set; }
        public string OrgName { get; set; }
    }
}
