using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Winit.Modules.Role.Model.Classes;

namespace Winit.Modules.Role.Model.Interfaces
{
    public interface IModulesMasterView
    {
        
        List<Module> Modules { get; set; }
        List<SubModule> SubModules { get; set; }
        List<SubSubModules> SubSubModules { get; set; }
    }
}
