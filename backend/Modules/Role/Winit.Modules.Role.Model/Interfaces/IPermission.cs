using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Winit.Modules.Base.Model;

namespace Winit.Modules.Role.Model.Interfaces
{
    public interface IPermission : IBaseModel
    {
         string RoleUid { get; set; }
         string SubSubModuleUid { get; set; }
         string Platform { get; set; }
         string SubModuleUid { get; set; }
         bool? FullAccess { get; set; }
         bool AddAccess { get; set; }
         bool EditAccess { get; set; }
         bool ViewAccess { get; set; }
         bool DeleteAccess { get; set; }
         bool DownloadAccess { get; set; }
         bool ApprovalAccess { get; set; }
         bool IsModified { get; set; }
        bool ShowInMenu { get; set; }

    }
}
