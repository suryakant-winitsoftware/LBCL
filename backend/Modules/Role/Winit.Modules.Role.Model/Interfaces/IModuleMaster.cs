using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Winit.Modules.Role.Model.Classes;

namespace Winit.Modules.Role.Model.Interfaces
{
    public interface IModuleMaster
    {
        List<IModule> Modules { get; set; }
        List<ISubModule> SubModules { get; set; }
        List<ISubSubModules> SubSubModules { get; set; }
    }
}
