using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Winit.Shared.Models.Common;
using Winit.Shared.Models.Enums;

namespace Winit.Modules.Emp.BL.Interfaces
{
    public interface IEmpInfoBL
    {
        Task<PagedResponse<Winit.Modules.Emp.Model.Interfaces.IEmpInfo>> GetEmpInfoDetails(List<SortCriteria> sortCriterias, int pageNumber,
        int pageSize, List<FilterCriteria> filterCriterias, bool isCountRequired);
        Task<Winit.Modules.Emp.Model.Interfaces.IEmpInfo> GetEmpInfoByUID(string UID);
        Task<int> CreateEmpInfo(Winit.Modules.Emp.Model.Interfaces.IEmpInfo createEmpInfo);
        Task<int> UpdateEmpInfoDetails(Winit.Modules.Emp.Model.Interfaces.IEmpInfo updateEmpInfo);
        Task<int> DeleteEmpInfoDetails(string UID);
    }
}
