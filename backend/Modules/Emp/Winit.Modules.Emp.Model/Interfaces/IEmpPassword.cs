using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Winit.Modules.Emp.Model.Interfaces
{
    public interface IEmpPassword
    {
        public string EmpUID { get; set; }
        public string EncryptedPassword { get; set; }
    }
}
