using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Winit.Modules.Base.Model;
using Winit.Modules.Emp.Model.Interfaces;

namespace Winit.Modules.Emp.Model.Classes
{
    public class EmpPassword:BaseModel, IEmpPassword
    {
        public string EmpUID { get; set; }
        public string EncryptedPassword { get; set; }
    }
}
