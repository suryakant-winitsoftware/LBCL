using Winit.Modules.PriceLadder.Model.Interfaces;
using Winit.Shared.Models.Common;
using Winit.Shared.Models.Enums;

namespace Winit.Modules.PriceLadder.BL.Interfaces
{
    public interface IPriceLadderingViewModel
    {
        public string ViewingMessage { get; set; }
        public int TotalPriceLadderRecord { get; set; }
        public int PageSize { get; set; }
        public int PageNumber { get; set; }
        List<Winit.Modules.PriceLadder.Model.Interfaces.IPriceLadderingItemView> PriceLadderingList { get; set; }
        List<Winit.Modules.PriceLadder.Model.Interfaces.IPriceLaddering> PriceLadderingsListToShowInGrid { get; set; }
        List<Winit.Modules.PriceLadder.Model.Interfaces.IPriceLaddering> PriceLadderingSubList { get; set; }
        List<Winit.Modules.SKU.Model.Interfaces.ISKU> PopUpSkuDetailsList { get; set; }
        Winit.Modules.PriceLadder.Model.Interfaces.IPriceLaddering PopUpPriceLadderingObj { get; set; }
        Winit.Modules.PriceLadder.Model.Interfaces.IPriceLaddering SelectedPriceLaddering { get; set; }
        Task GetThePriceLadderingList();
        Task OnProductCategoryClick(IPriceLaddering item, IPriceLaddering subItem);
        Task GetRelatedRowData();
        Task<List<ISelectionItem>> GetBroadClassificationDDValues();
        Task OnFilterApply(Dictionary<string, string> filterCriteria);
        Task<List<ISelectionItem>> GetAllDivisionDDValues();
        Task<List<ISelectionItem>> GetAllBranchDDLValues();
        Task<List<ISelectionItem>> GetOUDetailsFromAPIAsync();
        Task PageIndexChanged(int pageNumber);
        Task ApplySort(SortCriteria sortCriteria);
    }
}
