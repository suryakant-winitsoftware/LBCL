using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Winit.Modules.User.Model.Interfaces;

namespace Winit.Modules.User.Model.Classes
{
    public class EmpDTO : IEmpDTO
    {
        public Winit.Modules.Emp.Model.Interfaces.IEmp Emp { get; set; }
        public Winit.Modules.Emp.Model.Interfaces.IEmpInfo EmpInfo { get; set; }
        public Winit.Modules.JobPosition.Model.Interfaces.IJobPosition JobPosition { get; set; }
        public IEnumerable<Winit.Modules.Emp.Model.Interfaces.IEmpOrgMapping> EmpOrgMapping { get; set; }
      //  public Winit.Modules.Location.Model.Interfaces.ILocationTypeAndValue Location { get; set; }
        public Winit.Modules.FileSys.Model.Interfaces.IFileSys FileSys { get; set; }
        public List<Winit.Modules.FileSys.Model.Interfaces.IFileSys> FileSysList { get; set; }


    }

    public class EmpDTOModel 
    {
        public Winit.Modules.Emp.Model.Classes.Emp Emp { get; set; }
        public Winit.Modules.Emp.Model.Classes.EmpInfo EmpInfo { get; set; }
        public IEnumerable<Winit.Modules.Emp.Model.Classes.EmpOrgMapping> EmpOrgMapping { get; set; }
        public Winit.Modules.JobPosition.Model.Classes.JobPosition JobPosition { get; set; }
      //  public Winit.Modules.Location.Model.Interfaces.ILocationTypeAndValue Location { get; set; }
        public Winit.Modules.FileSys.Model.Classes.FileSys FileSys { get; set; }


    }
}
