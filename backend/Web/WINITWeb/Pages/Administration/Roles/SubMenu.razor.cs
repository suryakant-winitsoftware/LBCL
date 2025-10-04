using Microsoft.AspNetCore.Components;
using Nest;
using System.Globalization;
using System.Resources;
using Winit.Modules.Role.Model.Classes;
using Winit.Modules.Role.Model.Constants;
using Winit.Modules.Role.Model.Interfaces;
using Winit.Shared.CommonUtilities.Common;
using Winit.UIComponents.Common.Language;


namespace WinIt.Pages.Administration.Roles
{
    public partial class SubMenu
    {
        [Parameter]
        public bool IsCheckAll
        {
            get
            {
                return isCheckAll;
            }
            set
            {
                isCheckAll = value;
                _PermissionHeaderChecks.FullAccess = isCheckAll;
                _PermissionHeaderChecks.Add = isCheckAll;
                _PermissionHeaderChecks.Edit = isCheckAll;
                _PermissionHeaderChecks.View = isCheckAll;
                _PermissionHeaderChecks.Delete = isCheckAll;
                _PermissionHeaderChecks.Download = isCheckAll;
                _PermissionHeaderChecks.Approval = isCheckAll;
            }
        }
        [Parameter]
        public SubModuleHierarchy SubSubModule { get; set; }

        Winit.Modules.Role.Model.Classes.PermissionHeaderChecks _PermissionHeaderChecks = new();

        bool isCheckAll { get; set; }
        protected override async Task OnInitializedAsync()
        {
            //await checkitems();
            LoadResources(null, _languageService.SelectedCulture);
        }

        private async Task checkitems()
        {
            throw new NotImplementedException();
        }

        protected void OnCheckUncheckAll()
        {
            IsCheckAll = !IsCheckAll;
            foreach (var item in SubSubModule.SubSubModules)
            {
                if (!item.Permissions.IsModified)
                {
                    item.Permissions.IsModified = true;
                }
                item.Permissions.FullAccess = IsCheckAll;
                item.Permissions.ViewAccess = IsCheckAll;
                item.Permissions.EditAccess = IsCheckAll;
                item.Permissions.AddAccess = IsCheckAll;
                item.Permissions.DeleteAccess = IsCheckAll;
                item.Permissions.DownloadAccess = IsCheckAll;
                item.Permissions.ApprovalAccess = IsCheckAll;
            }
        }
       
        protected void CheckAllSinglePermission(string propertyName)
        {
            switch (propertyName)
            {
                case MenuConstants.FullAccess:
                    _PermissionHeaderChecks.FullAccess = !_PermissionHeaderChecks.FullAccess;
                    break;
                case MenuConstants.Add:
                    _PermissionHeaderChecks.Add = !_PermissionHeaderChecks.Add;
                    break;
                case MenuConstants.Edit:
                    _PermissionHeaderChecks.Edit = !_PermissionHeaderChecks.Edit;
                    break;
                case MenuConstants.View:
                    _PermissionHeaderChecks.View = !_PermissionHeaderChecks.View;
                    break;
                case MenuConstants.Delete:
                    _PermissionHeaderChecks.Delete = !_PermissionHeaderChecks.Delete;
                    break;
                case MenuConstants.Download:
                    _PermissionHeaderChecks.Download = !_PermissionHeaderChecks.Download;
                    break;
                case MenuConstants.Approval:
                    _PermissionHeaderChecks.Approval = !_PermissionHeaderChecks.Approval;
                    break;

            }
            foreach (var item in SubSubModule.SubSubModules)
            {
                if (item.Permissions.SubModuleUid.Equals(SubSubModule.SubModule.UID))
                {

                    if (!item.Permissions.IsModified)
                    {
                        item.Permissions.IsModified = true;
                    }

                    switch (propertyName)
                    {
                        case MenuConstants.FullAccess:
                            item.Permissions.FullAccess = _PermissionHeaderChecks.FullAccess;
                            break;
                        case MenuConstants.Add:
                            item.Permissions.AddAccess = _PermissionHeaderChecks.Add;
                            break;
                        case MenuConstants.Edit:
                            item.Permissions.EditAccess = _PermissionHeaderChecks.Edit;
                            break;
                        case MenuConstants.View:
                            item.Permissions.ViewAccess = _PermissionHeaderChecks.View;
                            break;
                        case MenuConstants.Delete:
                            item.Permissions.DeleteAccess = _PermissionHeaderChecks.Delete;
                            break;
                        case MenuConstants.Download:
                            item.Permissions.DownloadAccess = _PermissionHeaderChecks.Download;
                            break;
                        case MenuConstants.Approval:
                            item.Permissions.ApprovalAccess = _PermissionHeaderChecks.Approval;
                            break;
                    }
                }

            }
            if (!_PermissionHeaderChecks.FullAccess || !_PermissionHeaderChecks.Add|| !_PermissionHeaderChecks.Edit
                || !_PermissionHeaderChecks.View|| !_PermissionHeaderChecks.Delete|| !_PermissionHeaderChecks.Download|| !_PermissionHeaderChecks.Approval)
            {
                isCheckAll = false;
            }
        }
        protected void CheckUncheckSingleValue(string propertyName, SubSubModuleMasterHierarchy subSubModuleMasterHierarchy)
        {
            if (!subSubModuleMasterHierarchy.Permissions.IsModified)
            {
                subSubModuleMasterHierarchy.Permissions.IsModified = true;
            }
            switch (propertyName)
            {
                case MenuConstants.FullAccess:
                    subSubModuleMasterHierarchy.Permissions.FullAccess = !CommonFunctions.GetBooleanValue(subSubModuleMasterHierarchy.Permissions.FullAccess);
                    _PermissionHeaderChecks.FullAccess= SubSubModule.SubSubModules.Any(p=>p.Permissions.FullAccess==false|| p.Permissions.FullAccess == null) ?false:true;
                    break;
                case MenuConstants.Add:
                    subSubModuleMasterHierarchy.Permissions.AddAccess = !subSubModuleMasterHierarchy.Permissions.AddAccess;
                    _PermissionHeaderChecks.Add = SubSubModule.SubSubModules.Any(p => p.Permissions.AddAccess == false) ? false : true;
                    break;
                case MenuConstants.Edit:
                    subSubModuleMasterHierarchy.Permissions.EditAccess = !subSubModuleMasterHierarchy.Permissions.EditAccess;
                    _PermissionHeaderChecks.Edit = SubSubModule.SubSubModules.Any(p => p.Permissions.EditAccess == false) ? false : true;
                    break;
                case MenuConstants.View:
                    subSubModuleMasterHierarchy.Permissions.ViewAccess = !subSubModuleMasterHierarchy.Permissions.ViewAccess;
                    _PermissionHeaderChecks.View = SubSubModule.SubSubModules.Any(p => p.Permissions.ViewAccess == false) ? false : true;
                    break;
                case MenuConstants.Delete:
                    subSubModuleMasterHierarchy.Permissions.DeleteAccess = !subSubModuleMasterHierarchy.Permissions.DeleteAccess;
                    _PermissionHeaderChecks.Delete = SubSubModule.SubSubModules.Any(p => p.Permissions.DeleteAccess == false) ? false : true;
                    break;
                case MenuConstants.Download:
                    subSubModuleMasterHierarchy.Permissions.DownloadAccess = !subSubModuleMasterHierarchy.Permissions.DownloadAccess;
                    _PermissionHeaderChecks.Download = SubSubModule.SubSubModules.Any(p => p.Permissions.DownloadAccess == false) ? false : true;
                    break;
                case MenuConstants.Approval:
                    subSubModuleMasterHierarchy.Permissions.ApprovalAccess = !subSubModuleMasterHierarchy.Permissions.ApprovalAccess;
                    _PermissionHeaderChecks.Approval = SubSubModule.SubSubModules.Any(p => p.Permissions.ApprovalAccess == false) ? false : true;
                    break;
            }
            if (!CommonFunctions.GetBooleanValue(subSubModuleMasterHierarchy.Permissions.FullAccess) || !subSubModuleMasterHierarchy.Permissions.AddAccess || !subSubModuleMasterHierarchy.Permissions.EditAccess
                || !subSubModuleMasterHierarchy.Permissions.ViewAccess || !subSubModuleMasterHierarchy.Permissions.DeleteAccess
                || !subSubModuleMasterHierarchy.Permissions.DownloadAccess || !subSubModuleMasterHierarchy.Permissions.ApprovalAccess)
            {

                isCheckAll = false;

            }

        }
    }
}
