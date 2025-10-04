using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Winit.Modules.Emp.Model.Interfaces;

namespace Winit.Modules.Emp.Model.Classes
{
    public class EmpView :Emp, IEmpView
    {
        public string? Phone { get; set; }
    }
}
