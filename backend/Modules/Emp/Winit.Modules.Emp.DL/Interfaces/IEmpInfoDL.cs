using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Winit.Shared.Models.Common;
using Winit.Shared.Models.Enums;

namespace Winit.Modules.Emp.DL.Interfaces
{
    public interface IEmpInfoDL
    {
        Task<PagedResponse<Winit.Modules.Emp.Model.Interfaces.IEmpInfo>> GetEmpInfoDetails(List<SortCriteria> sortCriterias, int pageNumber,
          int pageSize, List<FilterCriteria> filterCriterias, bool isCountRequired);
        Task<Winit.Modules.Emp.Model.Interfaces.IEmpInfo> GetEmpInfoByUID(string UID, IDbConnection? connection = null, IDbTransaction? transaction = null);
        Task<int> CreateEmpInfo(Winit.Modules.Emp.Model.Interfaces.IEmpInfo createEmpInfo, IDbConnection? connection = null, IDbTransaction? transaction = null);
        Task<int> UpdateEmpInfoDetails(Winit.Modules.Emp.Model.Interfaces.IEmpInfo updateEmpInfo, IDbConnection? connection = null, IDbTransaction? transaction = null);
        Task<int> DeleteEmpInfoDetails(string UID, IDbConnection? connection = null, IDbTransaction? transaction = null);
    }
}
