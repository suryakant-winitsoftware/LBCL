using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Winit.Modules.Currency.DL.Interfaces;
using Winit.Modules.Currency.Model.Classes;
using Winit.Modules.Currency.Model.Interfaces;
using Winit.Modules.Emp.Model.Classes;
using Winit.Modules.Org.BL.Interfaces;
using Winit.Modules.User.BL.Interfaces;
using Winit.Modules.User.Model.Interface;
using Winit.Shared.CommonUtilities.Extensions;
using Winit.Shared.Models.Common;
using Winit.Shared.Models.Enums;

namespace Winit.Modules.User.BL.Classes
{
    public class MaintainUserBL : IMaintainUserBL
    {
        protected readonly Winit.Modules.User.DL.Interfaces.IMaintainUserDL _maintainUserDL;
        IServiceProvider _serviceProvider;
        private readonly IOrgBL orgBL;
        ICurrencyDL _currencyDL;
        public MaintainUserBL(DL.Interfaces.IMaintainUserDL maintainUserDL, ICurrencyDL currencyDL, IServiceProvider serviceProvider, IOrgBL orgBL)
        {
            _maintainUserDL = maintainUserDL;
            _serviceProvider = serviceProvider;
            this.orgBL = orgBL;
            _currencyDL = currencyDL;
        }
        public async Task<PagedResponse<Winit.Modules.User.Model.Interfaces.IMaintainUser>> SelectAllMaintainUserDetails(List<SortCriteria> sortCriterias, int pageNumber,
             int pageSize, List<FilterCriteria> filterCriterias, bool isCountRequired)
        {
            return await _maintainUserDL.SelectAllMaintainUserDetails(sortCriterias, pageNumber, pageSize, filterCriterias, isCountRequired);
        }
        public async Task<int> CUDEmployee(Winit.Modules.User.Model.Classes.EmpDTOModel empDTO, string encryptPassword)
        {
            return await _maintainUserDL.CUDEmployee(empDTO, encryptPassword);
        }
        public async Task<Winit.Modules.User.Model.Interfaces.IEmpDTO> SelectMaintainUserDetailsByUID(string empUID)
        {
            var (emp, empInfo, jobPosition, empOrgMapping, fileSys) = await _maintainUserDL.SelectMaintainUserDetailsByUID(empUID);
            Winit.Modules.User.Model.Interfaces.IEmpDTO empDTO = _serviceProvider.CreateInstance<Winit.Modules.User.Model.Interfaces.IEmpDTO>();
            if (emp != null)
            {
                empDTO.Emp = emp;
                empDTO.EmpInfo = empInfo;
                empDTO.JobPosition = jobPosition;
                empDTO.EmpOrgMapping = empOrgMapping;
                if (fileSys != null)
                {
                    empDTO.FileSys = fileSys;
                }

            }
            return empDTO;
        }
        public async Task<PagedResponse<Winit.Modules.User.Model.Interfaces.IUserRoles>> SelectUserRolesDetails(List<SortCriteria> sortCriterias, int pageNumber,
           int pageSize, List<FilterCriteria> filterCriterias, bool isCountRequired, string empUID)
        {
            return await _maintainUserDL.SelectUserRolesDetails(sortCriterias, pageNumber, pageSize, filterCriterias, isCountRequired, empUID);
        }
        public async Task<PagedResponse<Winit.Modules.User.Model.Interfaces.IUserFranchiseeMapping>> SelectUserFranchiseeMappingDetails(List<SortCriteria> sortCriterias, int pageNumber,
           int pageSize, List<FilterCriteria> filterCriterias, bool isCountRequired, string JobPositionUID, string OrgTypeUID, string ParentUID)
        {
            return await _maintainUserDL.SelectUserFranchiseeMappingDetails(sortCriterias, pageNumber, pageSize, filterCriterias, isCountRequired, JobPositionUID, OrgTypeUID, ParentUID);
        }

        public async Task<IUserMaster> GetAllUserMasterDataByLoginID(string LoginID)
        {

            IUserMaster userMaster = await _maintainUserDL.GetAllUserMasterDataByLoginID(LoginID);
            if (userMaster is not null)
            {
                userMaster.ProductDivisionSelectionItems = await orgBL.GetProductDivisionSelectionItems();
            }
            return userMaster;
        }
        public async Task<IEnumerable<Winit.Modules.Org.Model.Interfaces.IOrg>> GetAplicableOrgs(string orgUID)
        {
            return await _maintainUserDL.GetAplicableOrgs(orgUID);
        }

        public async Task<List<IUserHierarchy>> GetUserHierarchyForRule(string hierarchyType, string hierarchyUID, int ruleId)
        {
            return await _maintainUserDL.GetUserHierarchyForRule(hierarchyType, hierarchyUID, ruleId);
        }

    }
}
