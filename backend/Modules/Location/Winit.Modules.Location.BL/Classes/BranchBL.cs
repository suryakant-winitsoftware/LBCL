using Winit.Modules.Location.BL.Interfaces;
using Winit.Shared.Models.Common;
using Winit.Shared.Models.Enums;

namespace Winit.Modules.Location.BL.Classes
{
    public class BranchBL : IBranchBL
    {
        protected readonly DL.Interfaces.IBranchDL _branchBL;
        public BranchBL(DL.Interfaces.IBranchDL branchBL)
        {
            _branchBL = branchBL;
        }
       public async  Task<PagedResponse<Winit.Modules.Location.Model.Interfaces.IBranch>> SelectAllBranchDetails(List<SortCriteria> sortCriterias, int pageNumber,
    int pageSize, List<FilterCriteria> filterCriterias, bool isCountRequired)
        {
            return await _branchBL.SelectAllBranchDetails(sortCriterias, pageNumber, pageSize, filterCriterias, isCountRequired);
        }
      public async  Task<Winit.Modules.Location.Model.Interfaces.IBranch> GetBranchByUID(string UID)
        {
            return await _branchBL.GetBranchByUID(UID);
        }
        public async  Task<List<Winit.Modules.Location.Model.Interfaces.IBranch>> GetBranchsByJobPositionUid(string jobPositionUid)
        {
            return await _branchBL.GetBranchsByJobPositionUid(jobPositionUid);
        }
       public async Task<int> CreateBranch(Winit.Modules.Location.Model.Interfaces.IBranch createbranch)
        {
            return await _branchBL.CreateBranch(createbranch);
        }
      public async  Task<int> UpdateBranch(Winit.Modules.Location.Model.Interfaces.IBranch updatebranch)
        {
            return await _branchBL.UpdateBranch(updatebranch);
        }
       public async Task<int> DeleteBranch(string UID)
        {
            return await _branchBL.DeleteBranch(UID);
        }
        public async Task<List<Winit.Modules.Location.Model.Interfaces.IBranch>> GetBranchByLocationHierarchy(string state, string city, string locality)
        {
            return await _branchBL.GetBranchByLocationHierarchy(state, city, locality);
        }
    }
}
