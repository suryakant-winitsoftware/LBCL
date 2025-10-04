using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Winit.Modules.SKU.Model.Interfaces;
using Winit.Modules.Tax.Model.Interfaces;
using Winit.Modules.Tax.Model.UIInterfaces;
using Winit.Shared.Models.Common;
using Winit.Shared.Models.Enums;

namespace Winit.Modules.Tax.BL.UIInterfaces;

public interface ITaxViewModel
{
    public int PageNumber { get; set; }
    public int PageSize{ get; set; }
    public int TotalTaxItemsCount { get; set; }
    public List<ITaxItemView> TaxItemViews { get; set; }
    public List<ITaxSKUMapItemView> TaxSKUMapItemViews { get; set; }
    public List<ISKU> SKUList { get; set; }
    public List<ISKUAttributes> SkuAttributesList { get; set; }
    Task PopulateViewModel();
    Task PopulateAddEditTaxPage();
    Task ApplyFilter(List<Shared.Models.Enums.FilterCriteria> filterCriterias);
    Task ResetFilter();
    Task ApplySort(List<Shared.Models.Enums.SortCriteria> sortCriterias);
    Task PageIndexChanged(int pageNumber);
    Task GetExistingTaxWithTaxSkuMaps(string TaxUID);
    Task OnAddSKuSelectedItems(List<ISelectionItem> selectionItems, string taxUID, ActionType actionType);
    Task<bool> CreateTaxMaster(ITax tax);
    Task<bool> UpdateTaxMaster(ITax tax);
    ITax GetTax();
}
