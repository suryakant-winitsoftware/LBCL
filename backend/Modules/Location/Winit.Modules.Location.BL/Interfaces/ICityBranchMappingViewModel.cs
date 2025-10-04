using Winit.Modules.Location.Model.Interfaces;
using Winit.Shared.Models.Events;

namespace Winit.Modules.Location.BL.Interfaces
{
    public interface ICityBranchMappingViewModel 
    {
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
        public int TotalItemsCount { get; set; }
        public List<ICityBranch> cityBrancheList { get; set; }
        Task PopulateViewModel();
        Task PopulatetBranchDetails(string UID);
        Task InsertCityBranchMapping();
        Task ApplyFilter(List<Shared.Models.Enums.FilterCriteria> filterCriterias);
        Task ResetFilter();
        Task ApplySort(List<Shared.Models.Enums.SortCriteria> sortCriterias);
        Task PageIndexChanged(int pageNumber);
        List<Winit.Shared.Models.Common.ISelectionItem> BranchList { get; set; }
        Task SelectedBranchInDDL(DropDownEvent dropDownEvent);
    }
}
