using Elasticsearch.Net;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Winit.Modules.Emp.BL.Interfaces;
using Winit.Modules.Emp.DL.Interfaces;
using Winit.Modules.Emp.Model.Classes;
using Winit.Modules.Emp.Model.Interfaces;
using Winit.Shared.Models.Common;
using Winit.Shared.Models.Enums;

namespace Winit.Modules.Emp.BL.Classes
{
    public class EmpBL : IEmpBL
    {
        protected readonly DL.Interfaces.IEmpDL _EmpRepository = null;
        public EmpBL(DL.Interfaces.IEmpDL EmpRepository)
        {
            _EmpRepository = EmpRepository;
        }
        public async Task<PagedResponse<Winit.Modules.Emp.Model.Interfaces.IEmp>> GetEmpDetails(List<SortCriteria> sortCriterias, int pageNumber,
            int pageSize, List<FilterCriteria> filterCriterias, bool isCountRequired)
        {
            return await _EmpRepository.GetEmpDetails(sortCriterias, pageNumber, pageSize, filterCriterias, isCountRequired);
        }
        public async Task<Winit.Modules.Emp.Model.Interfaces.IEmp> GetEmpByUID(string UID)
        {
            return await _EmpRepository.GetEmpByUID(UID);
        }
        public async Task<Winit.Modules.Emp.Model.Interfaces.IEmp> GetBMByBranchUID(string UID)
        {
            return await _EmpRepository.GetBMByBranchUID(UID);
        }
        public async Task<int> CreateEmp(Winit.Modules.Emp.Model.Interfaces.IEmp createEmp, string encryptPassword)
        {
            return await _EmpRepository.CreateEmp(createEmp, encryptPassword);
        }
        public async Task<int> UpdateEmp(Winit.Modules.Emp.Model.Interfaces.IEmp updateEmp, string encryptPassword)
        {
            return await _EmpRepository.UpdateEmp(updateEmp, encryptPassword);
        }
        public async Task<int> DeleteEmp(string Code)
        {
            return await _EmpRepository.DeleteEmp(Code);
        }
        public async Task<Winit.Modules.Emp.Model.Interfaces.IEmp> GetEmpByLoginId(string LoginId)
        {
            return await _EmpRepository.GetEmpByLoginId(LoginId);
        }
        public async Task<IEmpPassword> GetPasswordByLoginId(string LoginId)
        {
            return await _EmpRepository.GetPasswordByLoginId(LoginId);
        }

        public async Task<IEmpInfo> GetEmpInfoByLoginId(string LoginId)
        {
            return await _EmpRepository.GetEmpInfoByLoginId(LoginId);
        }
        public async Task<List<Winit.Modules.Emp.Model.Interfaces.IEmp>> GetAllEmpDetailsByOrgUID(string OrgUID)
        {
            return await _EmpRepository.GetAllEmpDetailsByOrgUID(OrgUID);
        }
        public async Task<List<Winit.Modules.Emp.Model.Interfaces.IEmp>> GetEmployeesByRoleUID(string orgUID, string roleUID)
        {
            return await _EmpRepository.GetEmployeesByRoleUID(orgUID,roleUID);
        }

        public async Task<IEnumerable<IEmp>> GetReportsToEmployeesByRoleUID(string roleUID)
        {
            return await _EmpRepository.GetReportsToEmployeesByRoleUID(roleUID);
        }
        public async Task<List<IEmpView>> GetEmpViewByUID(string empUID)
        {
            return await _EmpRepository.GetEmpViewByUID(empUID);
        }
        public async Task<List<ISelectionItem>> GetEmployeesSelectionItemByRoleUID(string orgUID, string roleUID)
        {
            return await _EmpRepository.GetEmployeesSelectionItemByRoleUID(orgUID, roleUID);
        }
        public async Task<List<Winit.Modules.Emp.Model.Interfaces.IEmp>> GetASMList(string branchUID, string Code)
        {
            return await _EmpRepository.GetASMList(branchUID,Code);
        }
        public async Task<List<Winit.Modules.Emp.Model.Interfaces.IEmp>> GetAllASM()
        {
            return await _EmpRepository.GetAllASM();
        }
        public async Task<List<EscalationMatrixDto>> GetEscalationMatrixAsync(string jobPositionUid)
        {
            return await _EmpRepository.GetEscalationMatrixAsync(jobPositionUid);
        }
    }
}
