using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Winit.Modules.Base.Model;
using Winit.Shared.Models.Enums;

namespace Winit.Modules.Emp.Model.Interfaces
{
    public interface IEmpInfo:IBaseModel
    {
       
        public string EmpUID { get; set; }
        public string Email { get; set; }
        public string Phone { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public bool CanHandleStock { get; set; }
        public string ADGroup { get; set; }
        public string ADUsername { get; set; }
        public ActionType ActionType { get; set; }
    }
}
