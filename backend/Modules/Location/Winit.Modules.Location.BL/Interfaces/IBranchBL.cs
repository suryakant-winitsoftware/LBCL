using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Winit.Shared.Models.Common;
using Winit.Shared.Models.Enums;

namespace Winit.Modules.Location.BL.Interfaces
{
    public interface IBranchBL
    {
        Task<PagedResponse<Winit.Modules.Location.Model.Interfaces.IBranch>> SelectAllBranchDetails(List<SortCriteria> sortCriterias, int pageNumber,
        int pageSize, List<FilterCriteria> filterCriterias, bool isCountRequired);
        Task<Winit.Modules.Location.Model.Interfaces.IBranch> GetBranchByUID(string UID);
        Task<List<Winit.Modules.Location.Model.Interfaces.IBranch>> GetBranchsByJobPositionUid(string jobPositionUidID);
        Task<int> CreateBranch(Winit.Modules.Location.Model.Interfaces.IBranch createbranch);
        Task<int> UpdateBranch(Winit.Modules.Location.Model.Interfaces.IBranch updatebranch);
        Task<int> DeleteBranch(string UID);
        Task<List<Winit.Modules.Location.Model.Interfaces.IBranch>> GetBranchByLocationHierarchy(string state, string city, string locality);
    }
}
