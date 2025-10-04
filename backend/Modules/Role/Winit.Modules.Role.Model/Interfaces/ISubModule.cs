using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Winit.Modules.Base.Model;

namespace Winit.Modules.Role.Model.Interfaces
{
    public interface ISubModule : IBaseModel
    {
        int CheckInPermission { get; set; }
        string SubModuleNameEn { get; set; }
        string SubModuleNameOther { get; set; }
        string RelativePath { get; set; }
        string Platform { get; set; }
        int SerialNo { get; set; }
        string ModuleUid { get; set; }
        bool ShowInMenu { get; set; }
        bool IsSubSubMenuDisplay { get; set; }
    }
}
