using Microsoft.AspNetCore.Components;
using System;
using System.Collections.Generic;
using System.Linq;
using Winit.Modules.Role.Model.Classes;
using Winit.UIModels.Web.Breadcrum.Classes;

namespace WinIt.Pages.Administration.Roles
{
    public partial class MaintainWebMenu 
    {
        public bool IsCheckAll { get; set; }
        Winit.UIModels.Web.Breadcrum.Interfaces.IDataService? iDataBreadcrumbService;
        protected override async Task OnInitializedAsync()
        {
            LoadResources(null, _languageService.SelectedCulture);
            SetHeaderName();
            await _maintainMenu.PopulateViewModel();
           
            //await SetHeaderName();
        }
        
        //[CascadingParameter]
        //public EventCallback<WinIt.BreadCrum.Interfaces.IDataService> CallbackService { get; set; }
        public void SetHeaderName()
        {
            iDataBreadcrumbService = new DataServiceModel()
            {
                HeaderText = Localizer["maintain_user_roles"],
                BreadcrumList = new List<Winit.UIModels.Web.Breadcrum.Interfaces.IBreadCrum>()
                {
                    new BreadCrumModel() { SlNo = 1, Text = @Localizer["maintain_roles"], IsClickable = true, URL = "maintainUserRole" },
                    new BreadCrumModel() { SlNo = 1, Text = @Localizer["manage_web_menu"] }
                }
            };
        }

        protected void OnCheckUncheckAll(ModuleMasterHierarchy SubModules)
        {
            IsCheckAll = !IsCheckAll;
            foreach (var item in SubModules.SubModuleHierarchies)
            {
                foreach (var subsubModule in item.SubSubModules)
                {
                    subsubModule.Permissions.FullAccess = IsCheckAll;
                    subsubModule.Permissions.ViewAccess = IsCheckAll;
                    subsubModule.Permissions.EditAccess = IsCheckAll;
                    subsubModule.Permissions.AddAccess = IsCheckAll;
                    subsubModule.Permissions.DeleteAccess = IsCheckAll;
                    subsubModule.Permissions.DownloadAccess = IsCheckAll;
                    subsubModule.Permissions.ApprovalAccess = IsCheckAll;
                    subsubModule.Permissions.IsModified = true;
                }
            }
        }
    }
}
