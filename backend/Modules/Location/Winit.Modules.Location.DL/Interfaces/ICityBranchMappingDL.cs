using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Winit.Shared.Models.Common;
using Winit.Shared.Models.Enums;

namespace Winit.Modules.Location.DL.Interfaces
{
    public interface ICityBranchMappingDL
    {
        Task<PagedResponse<Winit.Modules.Location.Model.Interfaces.ICityBranch>> SelectCityBranchDetails(List<SortCriteria> sortCriterias, int pageNumber,
int pageSize, List<FilterCriteria> filterCriterias, bool isCountRequired);
        Task<List<ISelectionItem>> SelectBranchDetails();
        Task<int> CreateCityBranchMapping(List<Winit.Modules.Location.Model.Interfaces.ICityBranchMapping> cityBranchMappings);
    }
}
