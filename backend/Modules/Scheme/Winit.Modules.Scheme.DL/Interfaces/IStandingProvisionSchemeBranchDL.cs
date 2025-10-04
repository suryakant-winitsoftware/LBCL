using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Winit.Modules.Scheme.Model.Interfaces;
using Winit.Shared.Models.Common;
using Winit.Shared.Models.Enums;

namespace Winit.Modules.Scheme.DL.Interfaces
{
    public interface IStandingProvisionSchemeBranchDL
    {
        Task<PagedResponse<IStandingProvisionSchemeBranch>> SelectAllStandingProvisionBranches(List<SortCriteria> sortCriterias, int pageNumber,
       int pageSize, List<FilterCriteria> filterCriterias, bool isCountRequired);

        Task<IStandingProvisionSchemeBranch> GetStandingProvisionBranchByUID(string UID);

        Task<int> CreateStandingProvisionBranch(IStandingProvisionSchemeBranch standingProvisionBranch);

        Task<int> UpdateStandingProvisionBranch(IStandingProvisionSchemeBranch standingProvisionBranch);

        Task<int> DeleteStandingProvisionBranch(string UID);
    }
}
