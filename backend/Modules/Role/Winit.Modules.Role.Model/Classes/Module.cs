using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Winit.Modules.Base.Model;
using Winit.Modules.Role.Model.Interfaces;

namespace Winit.Modules.Role.Model.Classes
{
    public class Module : BaseModel, IModule
    {
        public string ModuleNameEn { get; set; }
        public string ModuleNameOther { get; set; }
        public string Platform { get; set; }
        public bool ShowInMenu { get; set; }
        public int SerialNo {  get; set; }

    }
}
