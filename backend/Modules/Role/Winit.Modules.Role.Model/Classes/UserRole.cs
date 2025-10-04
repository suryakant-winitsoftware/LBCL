using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Winit.Modules.Base.Model;
using Winit.Modules.Role.Model.Interfaces;

namespace Winit.Modules.Role.Model.Classes
{
    public class UserRole:BaseModel, IUserRole
    {
        public string Name { get; set; }
        public bool IsAdmin{ get; set; }
        public bool IsDistributorUser { get; set; }
        public bool IsPrincipleUser { get; set; }
        public bool IsAppUser { get; set; }
        public bool IsCPEUser { get; set; }
        public string AliasDisignation { get; set; }
    }
}
