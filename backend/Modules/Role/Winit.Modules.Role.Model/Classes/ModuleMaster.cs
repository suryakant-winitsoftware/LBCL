using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Winit.Modules.Role.Model.Interfaces;

namespace Winit.Modules.Role.Model.Classes
{
    public class ModuleMaster: IModuleMaster
    {
       public List<IModule> Modules { get; set; }
       public List<ISubModule> SubModules { get; set; }
       public List<ISubSubModules> SubSubModules { get; set; }
    }
}
