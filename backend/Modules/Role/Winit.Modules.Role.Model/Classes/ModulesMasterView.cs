using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Winit.Modules.Role.Model.Interfaces;

namespace Winit.Modules.Role.Model.Classes
{
    public class ModulesMasterView: IModulesMasterView
    {
       
        public List<Module> Modules { get; set; }
        public List<SubModule> SubModules { get; set; }
        public List<SubSubModules> SubSubModules { get; set; }
    }
}
