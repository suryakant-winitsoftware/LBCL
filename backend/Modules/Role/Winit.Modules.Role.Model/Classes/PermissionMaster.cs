using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Winit.Modules.Role.Model.Classes
{
    public class PermissionMaster
    {
        public string RoleUID { get; set; }
        public string Platform { get; set; }
        public bool IsPrincipalPermission { get; set; }
        public List<Permission> Permissions { get; set; }
    }
}
