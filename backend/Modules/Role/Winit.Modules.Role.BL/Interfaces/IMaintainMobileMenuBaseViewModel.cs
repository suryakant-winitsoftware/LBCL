using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Winit.Modules.Role.Model.Classes;

namespace Winit.Modules.Role.BL.Interfaces
{
    public interface IMaintainMobileMenuBaseViewModel
    {
        bool IsLoad { get; set; }
        string RoleName { get; set; }
        List<ModuleMasterHierarchy> ModuleMasters { get; set; }
        Task PopulateViewModel();
        Task SavePermissions();
        void SelectOrDeselectAllSubModules();
        void SelectOrDeselectSubSubModules(SubSubModuleMasterHierarchy subSubModuleMasterHierarchy, SubModuleHierarchy subModuleHierarchy, bool isYes);
        void SelectOrDeselectSubModules(SubModuleHierarchy subModuleHierarchy, bool isYes);
    }
}
