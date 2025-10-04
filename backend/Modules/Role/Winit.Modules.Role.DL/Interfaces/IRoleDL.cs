using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Winit.Modules.Role.Model.Classes;
using Winit.Modules.Role.Model.Interfaces;
using Winit.Shared.Models.Common;
using Winit.Shared.Models.Enums;

namespace Winit.Modules.Role.DL.Interfaces
{
    public interface IRoleDL
    {
        Task<ModulesMasterView> GetAllModulesMaster(string platform);
        Task<PagedResponse<IRole>> SelectAllRole(List<SortCriteria> sortCriterias, int pageNumber,
            int pageSize, List<FilterCriteria> filterCriterias, bool isCountRequired);
        Task<int> CreateRoles(IRole role);
        Task<int> UpdateRoles(IRole role);
        Task<List<IPermission>> SelectAllPermissions(string roleUID, string platform, bool isPrincipalTypePermission);
        Task<int> CUDPermissionMaster(PermissionMaster permissionMaster);
        Task<IEnumerable<IRole>> SelectRolesByOrgUID(string orguid, bool IsAppUser = false);
        Task<IRole> SelectRolesByUID(string uid);
        Task<int> UpdateMenuDataByRole(IRole role);

        Task<List<Winit.Modules.Role.Model.Interfaces.IModule>> SelectAllmodules(string platform);
        Task<List<ISubModule>> SelectAllSubModules(string platform);
        Task<List<ISubSubModules>> SelectAllSubSubModules(string platform);
        Task<IPermission> GetPermissionByRoleAndPage(string roleUID, string relativePath, bool isPrincipleRole)
        {
            throw new NotImplementedException();    
        }
    }
}
