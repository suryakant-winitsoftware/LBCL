using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Winit.Modules.Role.BL.Interfaces
{
    public interface IMenu
    {
        Winit.Modules.Role.Model.Interfaces.IMenuMasterHierarchyView ModulesMasterHierarchy { get; set; }
        Task PopulateMenuData();
    }
}
