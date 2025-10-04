using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Winit.Modules.Emp.Model.Classes
{
    public class EscalationMatrixDto
    {
        public string Name { get; set; }
        public string Code { get; set; }
        public string Mobile { get; set; }
        public string Designation { get; set; }
        public string Level { get; set; } // User, ATL, TL, etc.
    }
}
