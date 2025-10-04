using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Winit.Modules.Base.Model;
using Winit.Shared.Models.Enums;

namespace Winit.Modules.Emp.Model.Interfaces
{
    public interface IEmpOrgMapping : IBaseModel
    {
        public string EmpUID { get; set; }
        public string OrgUID { get; set; }
    }
}
