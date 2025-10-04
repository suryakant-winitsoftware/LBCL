using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Winit.Modules.Role.Model.Classes
{
    public class SubModuleHierarchy
    {
        public SubModule SubModule { get; set; }
        public Permission? SubModulePermissions { get; set; }
        public List<SubSubModuleMasterHierarchy> SubSubModules { get; set; }
    }
    public class SubModuleHierarchies
    {
        public SubModule SubModule { get; set; }
        public List<SubSubModules> SubSubModules { get; set; }
    }
}
