using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Winit.Modules.Emp.Model.Interfaces;
using Winit.Shared.Models.Common;
using Winit.Shared.Models.Enums;

namespace Winit.Modules.Emp.BL.Interfaces
{
    public interface IEmpOrgMappingBL
    {
        Task<PagedResponse<Winit.Modules.Emp.Model.Interfaces.IEmpOrgMapping>> GetEmpOrgMappingDetails(List<SortCriteria> sortCriterias, int pageNumber,
           int pageSize, List<FilterCriteria> filterCriterias, bool isCountRequired);
        Task<int> CreateEmpOrgMapping(List<Winit.Modules.Emp.Model.Classes.EmpOrgMapping> empOrgMappings);
        Task<int> DeleteEmpOrgMapping(string uid);
        Task<IEnumerable<Winit.Modules.Emp.Model.Interfaces.IEmpOrgMapping>> GetEmpOrgMappingDetailsByEmpUID(string empUID);
    }
}
