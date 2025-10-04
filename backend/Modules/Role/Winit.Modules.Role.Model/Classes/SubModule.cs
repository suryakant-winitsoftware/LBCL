using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Winit.Modules.Base.Model;

namespace Winit.Modules.Role.Model.Classes
{
    public class SubModule:BaseModel,Interfaces.ISubModule
    {
        public int CheckInPermission { get; set; }
        public string SubModuleNameEn { get; set; }
        public string SubModuleNameOther { get; set; }
        public string RelativePath { get; set; }
        public string Platform { get; set; }
        public int SerialNo { get; set; }
        public string ModuleUid { get; set; }
        public bool ShowInMenu {  get; set; }
        public bool IsSubSubMenuDisplay {  get; set; }

    }
}
