using Microsoft.AspNetCore.Components;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Winit.Modules.Base.BL;
using Winit.Modules.Common.BL.Interfaces;
using Winit.Modules.Role.Model.Classes;
using Winit.Modules.Role.Model.Interfaces;
using Winit.Shared.CommonUtilities.Common;
using Winit.Shared.Models.Common;
using Winit.UIComponents.Common;
using Winit.UIComponents.Common.Services;

namespace Winit.Modules.Role.BL.Classes
{
    public class MobileMenu : Interfaces.IMenu
    {
        public MobileMenu(Winit.Modules.Role.Model.Interfaces.IMenuMasterHierarchyView _ModulesMasterHierarchy, ApiService apiService, Winit.Shared.Models.Common.IAppConfig appConfigs, CommonFunctions commonFunctions, NavigationManager navigationManager,
           Winit.Modules.Common.Model.Interfaces.IDataManager dataManager, IAlertService alertService, ILoadingService loadingService, IAppUser appUser)
        {
            ModulesMasterHierarchy = _ModulesMasterHierarchy;

            this._apiService = apiService;
            this._appConfigs = appConfigs;
            this._commonFunctions = commonFunctions;
            this._navigationManager = navigationManager;
            this._dataManager = dataManager;
            this._alertService = alertService;
            _loadingService = loadingService;
            _appUser = appUser;
        }
        ILoadingService _loadingService;
        IAppUser _appUser;
        Winit.Shared.Models.Common.IAppConfig _appConfigs { get; set; }
        CommonFunctions _commonFunctions { get; set; }
        NavigationManager _navigationManager { get; set; }
        Common.Model.Interfaces.IDataManager _dataManager { get; set; }
        IAlertService _alertService { get; set; }
        ApiService _apiService { get; set; }
        public List<ModuleMasterHierarchy> ModuleMasters { get; set; } = new();
        public ModulesMasterView ModulesMasterView { get; set; }
        List<Permission> Permissions { get; set; } = new();
        public Winit.Modules.Role.Model.Interfaces.IMenuMasterHierarchyView ModulesMasterHierarchy { get; set; }
        public async Task PopulateMenuData()
        {
            await GetAllMenuDetails();
        }

        public async Task GetAllMenuDetails()
        {
            Winit.Shared.Models.Common.ApiResponse<string> apiResponse = await _apiService.FetchDataAsync($"{_appConfigs.ApiBaseUrl}Role/GetAllModulesMaster?Platform=Mobile", HttpMethod.Get);
            if (apiResponse != null)
            {
                if (apiResponse.IsSuccess && apiResponse.StatusCode == 200)
                {
                    ApiResponse<ModulesMasterView>? apiResponse1 = JsonConvert.DeserializeObject<ApiResponse<ModulesMasterView>>(apiResponse.Data);
                    if (apiResponse1.StatusCode == 200 && apiResponse1.Data != null)
                    {
                        ModulesMasterView = apiResponse1.Data;
                        _dataManager.SetData(nameof(IModulesMasterView), ModulesMasterView);
                        if (_appUser.Role.IsAdmin)
                        {
                            PrePareModuleMasterViewForMenu();
                        }
                        else
                        {
                            await GetPermissions();
                        }
                    }
                }
            }
        }

        public async Task GetPermissions()
        {
            try
            {
                ApiResponse<string> apiResponse = await _apiService.FetchDataAsync($"{_appConfigs.ApiBaseUrl}Role/SelectAllPermissionsByRoleUID?roleUID={_appUser.Role.UID}&platform=Mobile&isPrincipalTypePermission={_appUser.Role.IsPrincipalRole}", HttpMethod.Get);
                if (apiResponse != null)
                {
                    ApiResponse<List<Permission>>? apiResponse1 = JsonConvert.DeserializeObject<ApiResponse<List<Permission>>>(apiResponse.Data);
                    if (apiResponse1 != null && apiResponse1.IsSuccess)
                    {
                        if (apiResponse1.Data != null)
                        {
                            Permissions = apiResponse1.Data;
                        }
                        else
                        {
                            Permissions = new();
                        }
                        PrePareModuleMasterViewForMenu();

                    }
                }
            }
            catch
            {
                throw;
            }
        }
        protected void PrePareModuleMasterViewForMenu()
        {
            ModuleMasters = new();

            if (ModulesMasterView != null)
            {
                foreach (Module module in ModulesMasterView.Modules)
                {
                    ModuleMasterHierarchy moduleMasterHierarchy = new()
                    {
                        Module = module,
                        SubModuleHierarchies = new(),
                    };
                    foreach (SubModule subModule in ModulesMasterView.SubModules)
                    {
                        if (module.UID.Equals(subModule.ModuleUid) && subModule.ShowInMenu)
                        {
                            SubModuleHierarchy subModuleHierarchy = new()
                            {
                                SubModule = null,
                                SubSubModules = new(),
                            };
                            Permission? subPermission = Permissions.Find(p => p.SubSubModuleUid == subModule.UID);
                            if (!string.IsNullOrEmpty(subModule.RelativePath))
                            {
                                if (_appUser.Role.IsAdmin || (subPermission != null && CommonFunctions.GetBooleanValue(subPermission?.FullAccess)))
                                {
                                    subModuleHierarchy.SubModule = subModule;
                                }
                            }
                            else
                            {
                                foreach (SubSubModules subSubModules in ModulesMasterView.SubSubModules)
                                {
                                    if (subModule.UID.Equals(subSubModules.SubModuleUid) && subSubModules.ShowInMenu)
                                    {
                                        if (!string.IsNullOrEmpty(subSubModules.RelativePath)|| subSubModules.SerialNo>0)
                                        {
                                            Permission? subSubPermission = Permissions.Find(p => p.SubSubModuleUid == subSubModules.UID);
                                            if (_appUser.Role.IsAdmin || (subSubPermission != null && CommonFunctions.GetBooleanValue(subSubPermission.FullAccess)))
                                            {
                                                if (subModuleHierarchy.SubModule == null)
                                                {
                                                    subModuleHierarchy.SubModule = subModule;
                                                }
                                                SubSubModuleMasterHierarchy subSubModuleMasterHierarchy = new()
                                                {
                                                    SubSubModule = subSubModules,
                                                };
                                                subModuleHierarchy.SubSubModules.Add(subSubModuleMasterHierarchy);
                                            }
                                        }
                                    }
                                }
                            }

                            if (subModuleHierarchy.SubModule != null)
                            {
                                moduleMasterHierarchy.SubModuleHierarchies.Add(subModuleHierarchy);
                            }
                        }
                    }
                    ModuleMasters.Add(moduleMasterHierarchy);
                }
               // ModulesMasterHierarchy.ModuleMasterHierarchies = ModuleMasters;
                ModulesMasterHierarchy.IsLoad = true;
            }
        }
        protected void PrePareModuleMasterViewForMenu_Old()
        {
            ModuleMasters = new();

            if (ModulesMasterView != null)
            {
                foreach (Module module in ModulesMasterView.Modules)
                {
                    ModuleMasterHierarchy moduleMasterHierarchy = new()
                    {
                        Module = module,
                        SubModuleHierarchies = new(),
                    };
                    foreach (SubModule subModule in ModulesMasterView.SubModules)
                    {
                        if (module.UID.Equals(subModule.ModuleUid))
                        {

                            SubModuleHierarchy subModuleHierarchy = new();
                            subModuleHierarchy.SubModule = subModule;
                            subModuleHierarchy.SubSubModules = new();

                            Permission? subPermission = Permissions.Find(p => p.SubSubModuleUid == subModule.UID);
                            subModuleHierarchy.SubModulePermissions = subPermission == null ? new Permission() : subPermission;
                            if (subModule.SS != 0 || string.IsNullOrEmpty(subModule.RelativePath))
                            {
                                foreach (SubSubModules subSubModules in ModulesMasterView.SubSubModules)
                                {

                                    if (subModule.UID.Equals(subSubModules.SubModuleUid) && subSubModules.SS == 0)
                                    {
                                        SubSubModuleMasterHierarchy subSubModuleMasterHierarchy = new()
                                        {
                                            SubSubModule = subSubModules,
                                        };
                                        subModuleHierarchy.SubSubModules.Add(subSubModuleMasterHierarchy);

                                        if (subModule.SS != 0 || string.IsNullOrEmpty(subModule.RelativePath))
                                        {
                                            Permission? permission = Permissions.Find(p => p.SubSubModuleUid == subSubModules.UID);
                                            if (permission == null)
                                            {
                                                permission = new Permission();
                                            }
                                            else
                                            {
                                                if (permission.ShowInMenu && subModuleHierarchy?.SubModulePermissions?.FullAccess == null)
                                                {
                                                    subModuleHierarchy.SubModulePermissions.FullAccess = true;
                                                }
                                                subSubModuleMasterHierarchy.Permissions = permission;
                                            }
                                        }
                                    }
                                }
                            }
                            moduleMasterHierarchy.SubModuleHierarchies.Add(subModuleHierarchy);
                        }
                    }
                    ModuleMasters.Add(moduleMasterHierarchy);
                }
                //ModulesMasterHierarchy.ModuleMasterHierarchies = ModuleMasters;
                ModulesMasterHierarchy.IsLoad = true;
            }
        }
    }
}
