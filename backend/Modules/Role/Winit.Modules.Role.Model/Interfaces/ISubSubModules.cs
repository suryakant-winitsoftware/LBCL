using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Winit.Modules.Base.Model;

namespace Winit.Modules.Role.Model.Interfaces
{
    public interface ISubSubModules : IBaseModel
    {
        string SubSubModuleNameEn { get; set; }
        string SubSubModuleNameOther { get; set; }
        string RelativePath { get; set; }
        string SubModuleUid { get; set; }
        bool ShowInMenu { get; set; }
        int SerialNo { get; set; }
    }
}
