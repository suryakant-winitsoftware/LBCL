using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Winit.Modules.Base.Model;

namespace Winit.Modules.Role.Model.Interfaces
{
    public interface IModule : IBaseModel
    {
        string ModuleNameEn { get; set; }
        string ModuleNameOther { get; set; }
        string Platform { get; set; }
        bool ShowInMenu { get; set; }
        int SerialNo { get; set; }

    }
}
