using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Winit.Modules.Role.Model.Classes;
using Winit.Modules.Role.Model.Interfaces;
using Winit.Shared.Models.Common;
using Winit.Shared.Models.Enums;

namespace Winit.Modules.Role.BL.Interfaces
{
    public interface IRoleBL
    {
        Task<IModuleMaster> GetModulesMasterByPlatForm(string platform);
        Task<PagedResponse<IRole>> SelectAllRole(List<SortCriteria> sortCriterias, int pageNumber,
            int pageSize, List<FilterCriteria> filterCriterias, bool isCountRequired);
        Task<ModulesMasterView> GetAllModulesMaster(string platform);
        Task<int> CreateRoles(IRole role);
        Task<int> UpdateRoles(IRole role);
        Task<List<IPermission>> SelectAllPermissions(string roleUID, string platform, bool isPrincipalTypePermission);
        Task<int> CUDPermissionMaster(PermissionMaster permissionMaster);
        Task<IEnumerable<IRole>> SelectRolesByOrgUID(string orguid, bool IsAppUser = false);
        Task<IRole> SelectRolesByUID(string uid);
        Task<int> UpdateMenuByPlatForm( string platForm);
        Task<IPermission> GetPermissionByRoleAndPage(string roleUID, string relativePath, bool isPrincipleRole);
    }
}
