using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Winit.Modules.User.Model.Interfaces;

namespace Winit.Modules.User.Model.Classes
{
    public class UserRoles : IUserRoles
    {
        public string UID { get; set; }
        public string BusinessUnit { get; set; }
        public string Role { get; set; }
        public string RoleName { get; set; }
        public string Designation { get; set; }
        public string Department { get; set; }
        public int RoleType { get; set; }
        public string ReportsTo { get; set; }
        public string ReportsToName { get; set; }
        public string LocationName { get; set; }
        public bool HASEOT { get; set; }
        public string HASEOTstr { get; set; }
        public DateTime ModifiedTime { get; set; }
        public bool P1 { get; set; }
        public bool P2 { get; set; }
        public bool P3 { get; set; }
        public bool S { get; set; }

    }
}
