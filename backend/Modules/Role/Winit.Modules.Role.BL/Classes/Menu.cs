using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
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
    public class Menu : Interfaces.IMenu
    {


        public Winit.Modules.Role.Model.Interfaces.IMenuMasterHierarchyView ModulesMasterHierarchy { get; set; }

        public Menu(Winit.Modules.Role.Model.Interfaces.IMenuMasterHierarchyView _ModulesMasterHierarchy, ApiService apiService, Winit.Shared.Models.Common.IAppConfig appConfigs, CommonFunctions commonFunctions, NavigationManager navigationManager,
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
        NavigationManager NavigationManager;
        Winit.Modules.Base.BL.ILocalStorageService _localStorageService;
        AuthenticationStateProvider _authStateProvider;

        public async Task PopulateMenuData()
        {
            
            if (_appUser.Emp.LoginId.Equals("test")|| _appUser.Emp.LoginId.Equals("Test"))
            {
                ModulesMasterHierarchy.IsTestUserLoad = true;
            }
            else
            {
                ModulesMasterHierarchy.IsTestUserLoad = false;
            }
            await GetAllMenuDetails();
            if (!_appUser.Role.IsAdmin)
            {
                await GetPermissions();
            }
            PrePareModuleMasterViewForMenu();
            //ModulesMasterHierarchy.ModuleMasterHierarchies = ModuleMasters;
            ModulesMasterHierarchy.IsLoad = true;
        }
        public async Task GetAllMenuDetails()
        {
            Winit.Shared.Models.Common.ApiResponse<string> apiResponse = await _apiService.FetchDataAsync($"{_appConfigs.ApiBaseUrl}Role/GetAllModulesMaster", HttpMethod.Get);
            if (apiResponse != null)
            {
                if (apiResponse.IsSuccess && apiResponse.StatusCode == 200)
                {
                    ApiResponse<ModulesMasterView>? apiResponse1 = JsonConvert.DeserializeObject<ApiResponse<ModulesMasterView>>(apiResponse.Data);
                    if (apiResponse1.StatusCode == 200 && apiResponse1.Data != null)
                    {
                        ModulesMasterView = apiResponse1.Data;
                        _dataManager.SetData(nameof(IModulesMasterView), ModulesMasterView);
                    }
                }
            }
        }
        public async Task GetPermissions()
        {
            try
            {
                ApiResponse<string> apiResponse = await _apiService.FetchDataAsync($"{_appConfigs.ApiBaseUrl}Role/SelectAllPermissionsByRoleUID?roleUID={_appUser.Role.UID}&isPrincipalTypePermission={_appUser.Role.IsPrincipalRole}", HttpMethod.Get);
                if (apiResponse != null)
                {
                    ApiResponse<List<Permission>>? apiResponse1 = JsonConvert.DeserializeObject<ApiResponse<List<Permission>>>(apiResponse.Data);
                    if (apiResponse1 != null)
                    {
                        if (apiResponse1.IsSuccess && apiResponse1.Data != null)
                        {
                            Permissions = apiResponse1.Data;
                        }
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
                        if (module.UID.Equals(subModule.ModuleUid))
                        {
                            SubModuleHierarchy subModuleHierarchy = new();
                            subModuleHierarchy.SubModule = subModule;
                            subModuleHierarchy.SubSubModules = new();
                            foreach (SubSubModules subSubModules in ModulesMasterView.SubSubModules)
                            {
                                if (subModule.UID.Equals(subSubModules.SubModuleUid))
                                {
                                    Permission? permission = null;
                                    if (!_appUser.Role.IsAdmin)
                                    {
                                        permission = Permissions.Find(p => p.SubSubModuleUid.Equals(subSubModules.UID));
                                        if (permission == null)
                                        {
                                            permission = new Permission()
                                            {
                                                UID = Guid.NewGuid().ToString(),
                                                CreatedBy = _appUser.Emp.UID,
                                                CreatedTime = DateTime.Now,
                                                SubSubModuleUid = subSubModules.UID,
                                            };
                                        }
                                        permission.SubModuleUid = subModule.UID;
                                    }
                                    SubSubModuleMasterHierarchy subSubModuleMasterHierarchy = new()
                                    {
                                        SubSubModule = subSubModules,
                                        Permissions = permission,
                                    };
                                    subModuleHierarchy.SubSubModules.Add(subSubModuleMasterHierarchy);
                                }
                            }
                            moduleMasterHierarchy.SubModuleHierarchies.Add(subModuleHierarchy);
                        }
                    }
                    ModuleMasters.Add(moduleMasterHierarchy);
                }
            }
        }

    }
}
