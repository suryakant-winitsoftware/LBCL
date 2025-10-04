using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Winit.Modules.Scheme.BL.Interfaces;
using Winit.Modules.Scheme.DL.Interfaces;
using Winit.Modules.Scheme.Model.Interfaces;
using Winit.Shared.Models.Common;
using Winit.Shared.Models.Enums;

namespace Winit.Modules.Scheme.BL.Classes
{
    public class StandingProvisionSchemeBranchBL: IStandingProvisionSchemeBranchBL
    {
        private readonly IStandingProvisionSchemeBranchDL _standingProvisionSchemeBranchDL;

        public StandingProvisionSchemeBranchBL(IStandingProvisionSchemeBranchDL standingProvisionSchemeBranchDL)
        {
            _standingProvisionSchemeBranchDL = standingProvisionSchemeBranchDL;
        }

        public async Task<PagedResponse<IStandingProvisionSchemeBranch>> SelectAllStandingProvisionBranches(
            List<SortCriteria> sortCriterias,
            int pageNumber,
            int pageSize,
            List<FilterCriteria> filterCriterias,
            bool isCountRequired)
        {
            return await _standingProvisionSchemeBranchDL.SelectAllStandingProvisionBranches(
                sortCriterias,
                pageNumber,
                pageSize,
                filterCriterias,
                isCountRequired
            );
        }

        public async Task<IStandingProvisionSchemeBranch> GetStandingProvisionBranchByUID(string UID)
        {
            return await _standingProvisionSchemeBranchDL.GetStandingProvisionBranchByUID(UID);
        }

        public async Task<int> CreateStandingProvisionBranch(IStandingProvisionSchemeBranch standingProvisionBranch)
        {
            return await _standingProvisionSchemeBranchDL.CreateStandingProvisionBranch(standingProvisionBranch);
        }

        public async Task<int> UpdateStandingProvisionBranch(IStandingProvisionSchemeBranch standingProvisionBranch)
        {
            return await _standingProvisionSchemeBranchDL.UpdateStandingProvisionBranch(standingProvisionBranch);
        }

        public async Task<int> DeleteStandingProvisionBranch(string UID)
        {
            return await _standingProvisionSchemeBranchDL.DeleteStandingProvisionBranch(UID);
        }
    }
}
