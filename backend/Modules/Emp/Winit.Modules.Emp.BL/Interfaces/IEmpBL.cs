using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Winit.Modules.Emp.Model.Classes;
using Winit.Modules.Emp.Model.Interfaces;
using Winit.Shared.Models.Common;
using Winit.Shared.Models.Enums;

namespace Winit.Modules.Emp.BL.Interfaces
{
    public interface IEmpBL
    {
        Task<PagedResponse<Winit.Modules.Emp.Model.Interfaces.IEmp>> GetEmpDetails(List<SortCriteria> sortCriterias, int pageNumber,
        int pageSize, List<FilterCriteria> filterCriterias, bool isCountRequired);
        Task<Winit.Modules.Emp.Model.Interfaces.IEmp> GetEmpByUID(string UID);
        Task<Winit.Modules.Emp.Model.Interfaces.IEmp> GetBMByBranchUID(string UID);
        Task<int> CreateEmp(Winit.Modules.Emp.Model.Interfaces.IEmp createEmp, string encryptPassword);
        Task<int> UpdateEmp(Winit.Modules.Emp.Model.Interfaces.IEmp updateEmp, string encryptPassword);
        Task<int> DeleteEmp(string UID);
        Task<Winit.Modules.Emp.Model.Interfaces.IEmp> GetEmpByLoginId(string LoginId);
        Task<Winit.Modules.Emp.Model.Interfaces.IEmpInfo> GetEmpInfoByLoginId(string LoginId);
        Task<IEmpPassword> GetPasswordByLoginId(string LoginId);
        Task<List<Winit.Modules.Emp.Model.Interfaces.IEmp>> GetAllEmpDetailsByOrgUID(string OrgUID);
        Task<IEnumerable<IEmp>> GetReportsToEmployeesByRoleUID(string orgUID);
        Task<List<IEmp>> GetEmployeesByRoleUID(string orgUID, string roleUID);
        Task<List<ISelectionItem>> GetEmployeesSelectionItemByRoleUID(string orgUID, string roleUID);
        Task<List<IEmpView>> GetEmpViewByUID(string empUID);
        Task<List<Winit.Modules.Emp.Model.Interfaces.IEmp>> GetASMList(string branchUID,string Code);
        Task<List<Winit.Modules.Emp.Model.Interfaces.IEmp>> GetAllASM();
        Task<List<EscalationMatrixDto>> GetEscalationMatrixAsync(string jobPositionUid);
    }
}
