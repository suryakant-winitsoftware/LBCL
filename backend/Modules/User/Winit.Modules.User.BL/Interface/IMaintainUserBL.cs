using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Winit.Modules.User.Model.Interface;
using Winit.Shared.Models.Common;
using Winit.Shared.Models.Enums;

namespace Winit.Modules.User.BL.Interfaces
{
    public interface IMaintainUserBL
    {
        Task<PagedResponse<Winit.Modules.User.Model.Interfaces.IMaintainUser>> SelectAllMaintainUserDetails(List<SortCriteria> sortCriterias, int pageNumber,
             int pageSize, List<FilterCriteria> filterCriterias, bool isCountRequired);
        Task<int> CUDEmployee(Winit.Modules.User.Model.Classes.EmpDTOModel empDTO,string encryptPassword);
        Task<Winit.Modules.User.Model.Interfaces.IEmpDTO> SelectMaintainUserDetailsByUID(string empUID);
        Task<PagedResponse<Winit.Modules.User.Model.Interfaces.IUserRoles>> SelectUserRolesDetails(List<SortCriteria> sortCriterias, int pageNumber,
           int pageSize, List<FilterCriteria> filterCriterias, bool isCountRequired, string empUID);

        Task<PagedResponse<Winit.Modules.User.Model.Interfaces.IUserFranchiseeMapping>> SelectUserFranchiseeMappingDetails(List<SortCriteria> sortCriterias, int pageNumber,
           int pageSize, List<FilterCriteria> filterCriterias, bool isCountRequired, string JobPositionUID, string OrgTypeUID, string ParentUID);
        Task<IUserMaster> GetAllUserMasterDataByLoginID(string LoginID);
        Task<IEnumerable<Winit.Modules.Org.Model.Interfaces.IOrg>> GetAplicableOrgs(string orgUID);
        Task<List<IUserHierarchy>> GetUserHierarchyForRule(string hierarchyType, string hierarchyUID, int ruleId);
    }
}
