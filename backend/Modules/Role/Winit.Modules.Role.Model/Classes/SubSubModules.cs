using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Winit.Modules.Base.Model;
using Winit.Modules.Role.Model.Interfaces;

namespace Winit.Modules.Role.Model.Classes
{
    public class SubSubModules:BaseModel,ISubSubModules
    {
        public string SubSubModuleNameEn { get; set; }
        public string SubSubModuleNameOther { get; set; }
        public string RelativePath { get; set; }
        public string SubModuleUid { get; set; }
        public bool ShowInMenu {  get; set; }
        public int SerialNo {  get; set; }
    }
}
