using Microsoft.Identity.Client;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Winit.Modules.Role.BL.Interfaces;
using Winit.Modules.Role.DL.Interfaces;
using Winit.Modules.Role.Model.Classes;
using Winit.Modules.Role.Model.Interfaces;
using Winit.Shared.CommonUtilities.Common;
using Winit.Shared.Models.Common;
using Winit.Shared.Models.Enums;

namespace Winit.Modules.Role.BL.Classes
{
    public class RoleBL : IRoleBL
    {
        IRoleDL _roleDL;
        public RoleBL(IRoleDL roleDL)
        {
            _roleDL = roleDL;
        }
        public async Task<ModulesMasterView> GetAllModulesMaster(string platform)
        {
            return await _roleDL.GetAllModulesMaster(platform);
        }
        public async Task<IModuleMaster> GetModulesMasterByPlatForm(string platform)
        {
            IModuleMaster moduleMaster = new ModuleMaster();
            moduleMaster.Modules = await _roleDL.SelectAllmodules(platform);
            moduleMaster.SubModules = await _roleDL.SelectAllSubModules(platform);
            moduleMaster.SubSubModules = await _roleDL.SelectAllSubSubModules(platform);
            return moduleMaster;
        }

        public async Task<PagedResponse<IRole>> SelectAllRole(List<SortCriteria> sortCriterias, int pageNumber,
            int pageSize, List<FilterCriteria> filterCriterias, bool isCountRequired)
        {
            return await _roleDL.SelectAllRole(sortCriterias, pageNumber, pageSize, filterCriterias, isCountRequired);
        }
        public async Task<int> CreateRoles(IRole role)
        {
            return await _roleDL.CreateRoles(role);
        }
        public async Task<int> UidateRoles(IRole role)
        {
            return await _roleDL.CreateRoles(role);
        }
        public async Task<IEnumerable<IRole>> SelectRolesByOrgUID(string orguid, bool IsAppUser = false)
        {
            return await _roleDL.SelectRolesByOrgUID(orguid, IsAppUser);

        }
        public async Task<IRole> SelectRolesByUID(string uid)
        {
            return await _roleDL.SelectRolesByUID(uid);
        }
        public async Task<int> UpdateRoles(IRole role)
        {
            return await _roleDL.UpdateRoles(role);
        }
        public async Task<List<IPermission>> SelectAllPermissions(string roleUID, string platform, bool isPrincipalTypePermission)
        {
            return await _roleDL.SelectAllPermissions(roleUID, platform, isPrincipalTypePermission);
        }
        public async Task<int> CUDPermissionMaster(PermissionMaster permissionMaster)
        {
            int insCount = await _roleDL.CUDPermissionMaster(permissionMaster);
            if (insCount < permissionMaster.Permissions.Count)
            {
                throw new Exception($"{permissionMaster.Permissions.Count - insCount} Records not saved!");
            }
            if (insCount == permissionMaster.Permissions.Count || permissionMaster.RoleUID.ToLower().Equals("admin"))
            {
                IModuleMaster? modulesMasterView = await GetModulesMasterByPlatForm(permissionMaster.Platform);
                insCount += permissionMaster.Platform.ToLower().Equals("web")
                   ? await PrePareModuleMasterViewForWebMenu(permissionMaster.RoleUID, permissionMaster.Platform, permissionMaster.IsPrincipalPermission, modulesMasterView)
                   : await PrePareModuleMasterViewForMobileMenu(permissionMaster.RoleUID, permissionMaster.Platform, permissionMaster.IsPrincipalPermission);
            }
            return insCount;
        }
        public async Task<int> UpdateMenuByPlatForm(string platForm)
        {
            var roles = await _roleDL.SelectAllRole(null, 0, 0, null, true);
            int count = 0;
            IModuleMaster? modulesMasterView = await GetModulesMasterByPlatForm(platForm);
            if (roles != null)
            {
                foreach (var role in roles.PagedData)
                {
                    count += await PrePareModuleMasterViewForWebMenu(role.UID, platForm, role.IsPrincipalRole, modulesMasterView);
                }
            }
            return count;
        }
        protected async Task<int> PrePareModuleMasterViewForWebMenu(string roleUID, string platForm, bool isPrincipalTypePermission, IModuleMaster? modulesMasterView)
        {
            List<IPermission> permissions = await SelectAllPermissions(roleUID, platForm, isPrincipalTypePermission);
            int retval = 0;
            bool isAdmin = roleUID.ToLower().Equals("admin");

            List<MenuHierarchy> ModuleMasters = new();
            if (modulesMasterView != null)
            {
                foreach (Module module in modulesMasterView.Modules)
                {
                    MenuHierarchy moduleMasterHierarchy = new()
                    {
                        Module = module,
                        SubModuleHierarchies = new(),
                    };

                    foreach (SubModule subModule in modulesMasterView.SubModules)
                    {
                        if (module.UID.Equals(subModule.ModuleUid))
                        {
                            SubModuleHierarchies subModuleHierarchy = new();
                            subModuleHierarchy.SubModule = subModule;
                            subModuleHierarchy.SubSubModules = new();
                            foreach (SubSubModules subSubModules in modulesMasterView.SubSubModules)
                            {
                                SubSubModuleMasterHierarchy subSubModuleMasterHierarchy = new();
                                if (subModule.UID.Equals(subSubModules.SubModuleUid))
                                {
                                    //if (isAdmin)
                                    //{
                                    //    subModuleHierarchy.sub_Sub_Modules.Add( sub_Sub_Modules);
                                    //}
                                    //else
                                    //{
                                    IPermission? permission = permissions?.Find(p => p.SubSubModuleUid.Equals(subSubModules.UID));
                                    if (permission != null && permission.SubSubModuleUid.Equals(subSubModules.UID))
                                    {
                                        if (permission.ViewAccess || CommonFunctions.GetBooleanValue(permission.FullAccess))
                                        {
                                            subModuleHierarchy.SubSubModules.Add(subSubModules);
                                        }
                                    }
                                    //}
                                }
                            }
                            if (subModuleHierarchy.SubSubModules != null && subModuleHierarchy.SubSubModules.Count > 0)
                            {
                                moduleMasterHierarchy.SubModuleHierarchies.Add(subModuleHierarchy);
                            }
                        }
                    }
                    if (module.SerialNo == 1 || (moduleMasterHierarchy.SubModuleHierarchies != null && moduleMasterHierarchy.SubModuleHierarchies.Count > 0))
                    {
                        ModuleMasters.Add(moduleMasterHierarchy);
                    }
                }

                IRole role = await SelectRolesByUID(roleUID);
                if (role != null)
                {
                    role.WebMenuData = JsonConvert.SerializeObject(ModuleMasters);
                    role.ServerModifiedTime = DateTime.Now;
                    retval += await _roleDL.UpdateMenuDataByRole(role);
                }
            }
            return retval;
        }
        protected async Task<int> PrePareModuleMasterViewForMobileMenu(string roleUID, string platForm, bool isPrincipalTypePermission)
        {
            List<IPermission> permissions = await _roleDL.SelectAllPermissions(roleUID, platForm, isPrincipalTypePermission);
            int retval = 0;
            bool isAdmin = roleUID.ToLower().Equals("admin");
            ModulesMasterView? modulesMasterView = await GetAllModulesMaster(platForm);
            List<MenuHierarchy> ModuleMasters = new();
            if (modulesMasterView != null)
            {
                foreach (Module module in modulesMasterView.Modules)
                {
                    MenuHierarchy moduleMasterHierarchy = new()
                    {
                        Module = module,
                        SubModuleHierarchies = new(),
                    };
                    foreach (SubModule subModule in modulesMasterView.SubModules)
                    {
                        if (module.UID.Equals(subModule.ModuleUid))
                        {
                            SubModuleHierarchies subModuleHierarchy = new();
                            subModuleHierarchy.SubModule = subModule;
                            subModuleHierarchy.SubSubModules = new();
                            foreach (SubSubModules sub_Sub_Modules in modulesMasterView.SubSubModules)
                            {
                                SubSubModuleMasterHierarchy subSubModuleMasterHierarchy = new();
                                if (subModule.UID.Equals(sub_Sub_Modules.SubModuleUid))
                                {
                                    //if (isAdmin)
                                    //{
                                    //    if (subModule.uid.Equals("MobileMenu"))
                                    //    {
                                    //        subModuleHierarchy.SubModule = new() { submodule_name_en = sub_Sub_Modules.sub_sub_module_name_en, submodule_name_other = sub_Sub_Modules.sub_sub_module_name_other, relative_path = sub_Sub_Modules.relative_path, show_in_menu = sub_Sub_Modules.show_in_menu };
                                    //        moduleMasterHierarchy.SubModuleHierarchies.Add(subModuleHierarchy);
                                    //    }
                                    //    else
                                    //    {
                                    //        subModuleHierarchy.sub_Sub_Modules.Add(sub_Sub_Modules);
                                    //    }
                                    //}
                                    //else
                                    //{
                                    if (subModule.UID.Equals("MobileMenu"))
                                    {
                                        IPermission? permission = permissions?.Find(p => p.SubSubModuleUid.Equals(sub_Sub_Modules.UID));
                                        if (permission != null && permission.SubSubModuleUid.Equals(sub_Sub_Modules.UID) && CommonFunctions.GetBooleanValue(permission?.FullAccess))
                                        {
                                            // Add as SubSubModule under the MobileMenu SubModule
                                            subModuleHierarchy.SubSubModules.Add(sub_Sub_Modules);
                                        }

                                    }
                                    else
                                    {
                                        IPermission? permission = permissions?.Find(p => p.SubSubModuleUid.Equals(sub_Sub_Modules.UID));
                                        if (permission != null && permission.SubSubModuleUid.Equals(sub_Sub_Modules.UID))
                                        {
                                            if (CommonFunctions.GetBooleanValue(permission?.FullAccess)) 
                                            {
                                                subModuleHierarchy.SubSubModules.Add(sub_Sub_Modules);
                                            }
                                        }
                                    }
                                    // }
                                }
                            }
                            if (subModuleHierarchy.SubSubModules != null && subModuleHierarchy.SubSubModules.Count > 0)
                            {
                                moduleMasterHierarchy.SubModuleHierarchies.Add(subModuleHierarchy);
                            }
                        }
                    }
                    if (moduleMasterHierarchy.SubModuleHierarchies != null && moduleMasterHierarchy.SubModuleHierarchies.Count > 0)
                    {
                        ModuleMasters.Add(moduleMasterHierarchy);
                    }
                }

                IRole role = await SelectRolesByUID(roleUID);
                if (role != null)
                {
                    role.MobileMenuData = JsonConvert.SerializeObject(ModuleMasters);
                    role.ServerModifiedTime = DateTime.Now;
                    retval += await _roleDL.UpdateMenuDataByRole(role);
                }
            }
            return retval;
        }
        public async Task<IPermission> GetPermissionByRoleAndPage(string roleUID, string relativePath, bool isPrincipleRole)
        {
            return await _roleDL.GetPermissionByRoleAndPage(roleUID, relativePath, isPrincipleRole);
        }

    }
}
