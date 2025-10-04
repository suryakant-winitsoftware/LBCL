using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Winit.Modules.Emp.Model.Classes;
using Winit.Modules.Emp.Model.Interfaces;
using Winit.Shared.Models.Common;
using Winit.Shared.Models.Enums;

namespace Winit.Modules.Emp.DL.Interfaces
{
    public interface IEmpDL
    {
        Task<PagedResponse<Winit.Modules.Emp.Model.Interfaces.IEmp>> GetEmpDetails(List<SortCriteria> sortCriterias, int pageNumber,
         int pageSize, List<FilterCriteria> filterCriterias, bool isCountRequired);
        Task<Winit.Modules.Emp.Model.Interfaces.IEmp> GetEmpByUID(string UID);
        Task<Winit.Modules.Emp.Model.Interfaces.IEmp> GetBMByBranchUID(string UID);
        Task<int> CreateEmp(Winit.Modules.Emp.Model.Interfaces.IEmp createEmp,string encryptPassword, IDbConnection? connection = null, IDbTransaction? transaction = null);
        Task<int> UpdateEmp(Winit.Modules.Emp.Model.Interfaces.IEmp updateEmp,string encryptPassword, IDbConnection? connection = null, IDbTransaction? transaction = null);
        Task<int> DeleteEmp(string UID, IDbConnection? connection = null, IDbTransaction? transaction = null);

        Task<Winit.Modules.Emp.Model.Interfaces.IEmp> GetEmpByLoginId(string LoginId);
        Task<IEmpPassword> GetPasswordByLoginId(string LoginId);
        Task CheckProcedure();
        Task<Winit.Modules.Emp.Model.Interfaces.IEmpInfo> GetEmpInfoByLoginId(string LoginId);
        Task<List<Winit.Modules.Emp.Model.Interfaces.IEmp>> GetAllEmpDetailsByOrgUID(string OrgUID);
        Task<IEnumerable<IEmp>> GetReportsToEmployeesByRoleUID(string roleUID);
        Task<List<Winit.Modules.Emp.Model.Interfaces.IEmp>> GetEmployeesByRoleUID(string orgUID,string roleUID);
        Task<List<ISelectionItem>> GetEmployeesSelectionItemByRoleUID(string orgUID, string roleUID);
        Task<List<IEmpView>> GetEmpViewByUID(string empUID);
        Task<List<Winit.Modules.Emp.Model.Interfaces.IEmp>> GetASMList(string branchUID, string Code);
        Task<List<Winit.Modules.Emp.Model.Interfaces.IEmp>> GetAllASM();
        Task<List<EscalationMatrixDto>> GetEscalationMatrixAsync(string jobPositionUid);
    }
}
