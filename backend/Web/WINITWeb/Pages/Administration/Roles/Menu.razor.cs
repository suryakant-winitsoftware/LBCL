using Microsoft.AspNetCore.Components;
using Winit.Modules.Role.Model.Classes;
using Winit.Modules.Role.Model.Interfaces;

namespace WinIt.Pages.Administration.Roles
{
    public partial class Menu
    {

        [Parameter]
        public bool IsCheckAll { get; set; }
        [Parameter]
        public ModuleMasterHierarchy SubModules { get; set; }
        [Parameter]
        public string UID {  get; set; }
        
    }
}
