using Winit.Modules.SKUClass.Model.Interfaces;
using Winit.Shared.Models.Enums;

namespace Winit.Modules.SKUClass.BL.UIInterfaces
{
    public interface ISKUClassGroupViewModel
    {
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
        public int TotalSKUClassGroupItemsCount { get; set; }
        public ISKUClassGroup? SKUClassGroup { get; set; }
        public List<ISKUClassGroup> SKUClassGroupsList { get; set; }
        public List<FilterCriteria> SKUClassGroupFilterCriterias { get; set; }
        public List<SortCriteria> SKUClassGroupSortCriterials { get; set; }
        Task PopulateViewModel();
        Task ApplyFilter(IDictionary<string, string> keyValuePairs);
        Task ResetFilter();
        Task ApplySort(List<Shared.Models.Enums.SortCriteria> sortCriterias);
        Task PageIndexChanged(int pageNumber);
        Task<bool> OnSKUClassGroupDeleteClick(ISKUClassGroup skuClassGroup);
    }
}
