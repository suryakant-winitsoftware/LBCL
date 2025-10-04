using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Winit.Modules.Base.Model;

namespace Winit.Modules.Role.Model.Classes
{
    public class Permission : BaseModel, Interfaces.IPermission
    {
        public string RoleUid { get; set; }
        public string SubSubModuleUid { get; set; }
        public string Platform { get; set; }
        public string SubModuleUid { get; set; }
        public bool? FullAccess { get; set; }
        public bool AddAccess { get; set; }
        public bool EditAccess { get; set; }
        public bool ViewAccess { get; set; }
        public bool DeleteAccess { get; set; }
        public bool DownloadAccess { get; set; }
        public bool ApprovalAccess { get; set; }
        public bool IsModified { get; set; }
        public bool ShowInMenu { get; set; }

    }
}
