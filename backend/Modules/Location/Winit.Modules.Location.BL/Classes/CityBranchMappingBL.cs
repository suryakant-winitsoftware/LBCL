using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Winit.Modules.Location.BL.Interfaces;
using Winit.Shared.Models.Common;
using Winit.Shared.Models.Enums;

namespace Winit.Modules.Location.BL.Classes
{
    public class CityBranchMappingBL : ICityBranchMappingBL
    {
        protected readonly DL.Interfaces.ICityBranchMappingDL _cityBranchMappingDL;
        public CityBranchMappingBL(DL.Interfaces.ICityBranchMappingDL cityBranchMappingDL)
        {
            _cityBranchMappingDL = cityBranchMappingDL;
        }

        public  async Task<PagedResponse<Winit.Modules.Location.Model.Interfaces.ICityBranch>> SelectCityBranchDetails(List<SortCriteria> sortCriterias, int pageNumber,
        int pageSize, List<FilterCriteria> filterCriterias, bool isCountRequired)
        {
            return await _cityBranchMappingDL.SelectCityBranchDetails(sortCriterias, pageNumber, pageSize, filterCriterias, isCountRequired);
        }
        public async Task<List<ISelectionItem>> SelectBranchDetails()
        {
            return await _cityBranchMappingDL.SelectBranchDetails();
        }
        public async Task<int> CreateCityBranchMapping(List<Winit.Modules.Location.Model.Interfaces.ICityBranchMapping> cityBranchMappings)
        {
            return await _cityBranchMappingDL.CreateCityBranchMapping(cityBranchMappings);
        }


    }
}
