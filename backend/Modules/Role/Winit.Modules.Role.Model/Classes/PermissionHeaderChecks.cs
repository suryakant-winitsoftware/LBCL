using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Winit.Modules.Role.Model.Classes
{
    public class PermissionHeaderChecks: Interfaces.IPermissionHeaderChecks
    {
        public bool FullAccess { get; set; }
        public bool Add { get; set; }
        public bool Edit { get; set; }
        public bool View { get; set; }
        public bool Delete { get; set; }
        public bool Download { get; set; }
        public bool Approval { get; set; }
    }
}
