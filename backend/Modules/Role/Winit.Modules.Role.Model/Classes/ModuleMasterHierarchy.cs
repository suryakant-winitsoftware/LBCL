using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Winit.Modules.Role.Model.Classes
{
    public class ModuleMasterHierarchy
    {
        public Module Module { get; set; }
        public List<SubModuleHierarchy> SubModuleHierarchies { get; set; }
    }
    public class MenuHierarchy
    {
        public bool IsClicked {  get; set; }
        public Module Module { get; set; }
        public List<SubModuleHierarchies> SubModuleHierarchies { get; set; }
    }

}
