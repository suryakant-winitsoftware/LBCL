using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Winit.Modules.Base.Model;


namespace Winit.Modules.User.Model.Interfaces
{
    public interface IEmpDTO
    {
      public Winit.Modules.Emp.Model.Interfaces.IEmp Emp { get; set; }
      public Winit.Modules.Emp.Model.Interfaces.IEmpInfo EmpInfo { get; set; }
      public Winit.Modules.JobPosition.Model.Interfaces.IJobPosition  JobPosition { get; set; }
     // public Winit.Modules.Location.Model.Interfaces.ILocationTypeAndValue Location { get; set; }
      public IEnumerable<Winit.Modules.Emp.Model.Interfaces.IEmpOrgMapping> EmpOrgMapping { get; set; }
      public Winit.Modules.FileSys.Model.Interfaces.IFileSys FileSys { get; set; }
      public List< Winit.Modules.FileSys.Model.Interfaces.IFileSys> FileSysList { get; set; }
    }
}
