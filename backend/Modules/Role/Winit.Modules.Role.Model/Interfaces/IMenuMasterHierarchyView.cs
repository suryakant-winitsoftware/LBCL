using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Winit.Modules.Role.Model.Classes;

namespace Winit.Modules.Role.Model.Interfaces
{
    public interface IMenuMasterHierarchyView
    {
        bool IsTestUserLoad { get; set; }
        bool IsLoad { get; set; }
        List<MenuHierarchy> ModuleMasterHierarchies { get; set; }
        Action<Winit.Modules.Role.Model.Interfaces.IMenuMasterHierarchyView> OnMenuAddigned { get; set; }
    }
}
