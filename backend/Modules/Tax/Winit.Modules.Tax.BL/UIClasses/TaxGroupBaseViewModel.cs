using Winit.Modules.Base.BL.Helper.Interfaces;
using Winit.Modules.Common.BL.Interfaces;
using Winit.Modules.Common.Model.Interfaces;
using Winit.Modules.Setting.BL.Interfaces;
using Winit.Modules.Tax.BL.UIInterfaces;
using Winit.Modules.Tax.Model.Classes;
using Winit.Modules.Tax.Model.Interfaces;
using Winit.Modules.Tax.Model.UIInterfaces;
using Winit.Shared.CommonUtilities.Extensions;
using Winit.Shared.Models.Common;
using Winit.Shared.Models.Enums;

namespace Winit.Modules.Tax.BL.UIClasses;
public abstract class TaxGroupBaseViewModel : ITaxGroupViewModel
{
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; }
    public int TotalTaxGroupItemsCount { get; set; }
    public List<ITaxGroupItemView> TaxGroupItemViews { get; set; }
    public ITaxGroup? SelectedTaxGroup { get; set; }
    public List<FilterCriteria> TaxGroupFilterCriterias { get; set; }
    public List<SortCriteria> TaxGroupSortCriterials { get; set; }
    public List<ITaxSelectionItem> TaxSelectionItems { get; set; }
    public List<ITaxSelectionItem> TaxSelectionItemsFromAPI { get; set; }
    public List<ISelectionItem> TaxSelectionItemsDD { get; set; }

    // Injection
    private readonly IServiceProvider _serviceProvider;
    private readonly IFilterHelper _filter;
    private readonly ISortHelper _sorter;

    private readonly IListHelper _listHelper;
    private readonly IAppUser _appUser;
    private readonly IAppSetting _appSetting;
    private readonly IDataManager _dataManager;

    protected TaxGroupBaseViewModel(IServiceProvider serviceProvider,
            IFilterHelper filter,
            ISortHelper sorter,
            IListHelper listHelper,
            IAppUser appUser,
            IAppSetting appSetting,
            IDataManager dataManager)
    {
        _serviceProvider = serviceProvider;
        _filter = filter;
        _sorter = sorter;
        _listHelper = listHelper;
        _appUser = appUser;
        _appSetting = appSetting;
        _dataManager = dataManager;
        // Initialize common properties or perform other common setup
        TaxGroupItemViews = new List<ITaxGroupItemView>();
        TaxGroupSortCriterials = new List<SortCriteria>();
        TaxGroupFilterCriterias = new List<FilterCriteria>();
        TaxSelectionItems = new List<ITaxSelectionItem>();
        TaxSelectionItemsFromAPI = new List<ITaxSelectionItem>();
        TaxSelectionItemsDD = new List<ISelectionItem>();
    }
    public async Task PopulateViewModel()
    {
        List<ITaxGroup> data = await GetTaxGroups_Data();
        if (data != null)
        {
            TaxGroupItemViews.Clear();
            TaxGroupItemViews.AddRange(ConvertToTaxGroupItemView(data));
        }
    }

    /// <summary>
    /// This will seach data from TaxGroupItemViews and store in FilteredTaxGroupItemViews & DisplayedTaxGroupItemViews
    /// </summary>
    /// <param name="filterCriterias"></param>
    /// <param name="filterMode"></param>
    /// <returns></returns>
    /// 

    public async Task ApplyFilter(List<Shared.Models.Enums.FilterCriteria> filterCriterias)
    {
        TaxGroupFilterCriterias.Clear();
        TaxGroupFilterCriterias.AddRange(filterCriterias);
        await PopulateViewModel();
    }
    public async Task ResetFilter()
    {
        TaxGroupFilterCriterias.Clear();
        await PopulateViewModel();
    }
    /// <summary>
    /// This will sort data from FilteredTaxGroupItemViews and store in FilteredTaxGroupItemViews & DisplayedTaxGroupItemViews
    /// </summary>
    /// <param name="sortCriterias"></param>
    /// <returns></returns>
    public async Task ApplySort(List<Shared.Models.Enums.SortCriteria> sortCriterias)
    {
        await Task.CompletedTask;
    }
    public async Task PageIndexChanged(int pageNumber)
    {
        PageNumber = pageNumber;
        await PopulateViewModel();
    }
    public async Task GetExistingTaxGroupWithByUID(string TaxGroupUID)
    {
        SelectedTaxGroup = await GetTaxGroupByUID_Data(TaxGroupUID);
        List<ITaxSelectionItem> taxSelectionItems = await GetTaxSelectionItems_Data(TaxGroupUID);
        if (taxSelectionItems != null && taxSelectionItems.Any())
        {
            TaxSelectionItems.Clear();
            TaxSelectionItems.AddRange(taxSelectionItems);
            TaxSelectionItemsFromAPI.AddRange(taxSelectionItems.Where(x => x.IsSelected));
            TaxSelectionItemsDD.Clear();
            TaxSelectionItemsDD.AddRange(ConvertToSelectionItem(TaxSelectionItems));
        }
    }
    public async Task<bool> CreateTaxGroupMaster(ITaxGroup taxGroup)
    {
        AddCreateFields(taxGroup, false);
        Winit.Modules.Tax.Model.Classes.TaxGroupMasterDTO taxGroupMaster = new()
        {
            TaxGroup = (TaxGroup)taxGroup
        };
        List<ITaxSelectionItem> taxSelectionItems = PrepareActionTypes();
        List<ITaxGroupTaxes> preparedTaxGroupTaxes = ConvertToITaxGroupTaxes(taxSelectionItems);
        taxGroupMaster.TaxGroupTaxes = preparedTaxGroupTaxes.OfType<TaxGroupTaxes>().ToList();
        return await CreateTaxGroupMaster_Data(taxGroupMaster);
    }
    private List<ITaxSelectionItem> PrepareActionTypes()
    {
        List<ITaxSelectionItem> taxSelectionItems = new();
        foreach (ITaxSelectionItem item in TaxSelectionItems)
        {
            ISelectionItem? dditem = TaxSelectionItemsDD.Find(i => i.UID == item.TaxUID);
            if (!dditem.IsSelected && item.IsSelected)
            {
                item.ActionType = ActionType.Delete;
                taxSelectionItems.Add(item);
                continue;
            }
            if (dditem.IsSelected && item.IsSelected)
            {
                item.ActionType = ActionType.Update;
                taxSelectionItems.Add(item);
                continue;
            }
            if (dditem.IsSelected && !item.IsSelected)
            {
                item.ActionType = ActionType.Add;
                taxSelectionItems.Add(item);
                continue;
            }
        }
        return taxSelectionItems;
    }
    public async Task<bool> UpdateTaxGroupMaster(ITaxGroup taxGroup)
    {
        AddUpdateFields(taxGroup);
        Winit.Modules.Tax.Model.Classes.TaxGroupMasterDTO taxGroupMaster = new()
        {
            TaxGroup = (TaxGroup)taxGroup
        };
        List<ITaxSelectionItem> taxSelectionItems = PrepareActionTypes();
        List<ITaxGroupTaxes> preparedTaxGroupTaxes = ConvertToITaxGroupTaxes(taxSelectionItems);
        taxGroupMaster.TaxGroupTaxes = preparedTaxGroupTaxes.OfType<TaxGroupTaxes>().ToList();
        return await UpdateTaxGroupMaster_Data(taxGroupMaster);
    }
    public async Task PrepareAddTaxGroupPage(ITaxGroup taxGroup)
    {
        SelectedTaxGroup = taxGroup;
        TaxSelectionItems.Clear();
        TaxSelectionItems.AddRange(await GetTaxSelectionItems_Data(""));
        TaxSelectionItemsDD.Clear();
        TaxSelectionItemsDD.AddRange(ConvertToSelectionItem(TaxSelectionItems));
    }
    public ITaxGroup GetTaxGroup()
    {
        return SelectedTaxGroup;
    }
    //ModelConversionMethods
    private List<ITaxGroupItemView> ConvertToTaxGroupItemView(List<ITaxGroup> taxGroups)
    {
        List<ITaxGroupItemView> taxGroupItemViews = new();
        foreach (ITaxGroup taxGroup in taxGroups)
        {
            taxGroupItemViews.Add(ConvertToTaxGroupItemView(taxGroup));
        }
        return taxGroupItemViews;
    }
    private ITaxGroupItemView ConvertToTaxGroupItemView(ITaxGroup taxGroup)
    {
        ITaxGroupItemView taxGroupItemView = _serviceProvider.CreateInstance<ITaxGroupItemView>();
        taxGroupItemView.Name = taxGroup.Name;
        taxGroupItemView.Code = taxGroup.Code;
        taxGroupItemView.ActionType = taxGroup.ActionType;
        taxGroupItemView.UID = taxGroup.UID;
        return taxGroupItemView;
    }
    private List<SelectionItem> ConvertToSelectionItem(List<ITaxSelectionItem> taxselectionItems)
    {
        List<SelectionItem> selectionItemsDD = new();
        foreach (ITaxSelectionItem item in taxselectionItems)
        {
            selectionItemsDD.Add(ConvertToSelectionItem(item));
        }
        return selectionItemsDD;
    }
    private SelectionItem ConvertToSelectionItem(ITaxSelectionItem taxSelectionItem)
    {
        SelectionItem selectionItem = new()
        {
            Label = taxSelectionItem.TaxName,
            Code = taxSelectionItem.UID,
            UID = taxSelectionItem.TaxUID,
            IsSelected = taxSelectionItem.IsSelected
        };
        return selectionItem;
    }
    private ITaxGroupTaxes ConvertToITaxGroupTaxes(ITaxSelectionItem taxSelectionItem)
    {
        Model.Interfaces.ITaxGroupTaxes taxGroupTaxes = _serviceProvider.CreateInstance<Model.Interfaces.ITaxGroupTaxes>();
        taxGroupTaxes.TaxGroupUID = SelectedTaxGroup.UID;
        taxGroupTaxes.TaxUID = taxSelectionItem.TaxUID;
        taxGroupTaxes.ActionType = taxSelectionItem.ActionType;
        if (taxSelectionItem.ActionType == ActionType.Add)
        {
            AddCreateFields(taxGroupTaxes, true);
        }
        if (taxSelectionItem.ActionType == ActionType.Update)
        {
            AddUpdateFields(taxGroupTaxes);
        }
        if (taxSelectionItem.ActionType == ActionType.Delete)
        {
            taxGroupTaxes.UID = taxSelectionItem.UID;
        }
        return taxGroupTaxes;
    }
    private List<ITaxGroupTaxes> ConvertToITaxGroupTaxes(List<ITaxSelectionItem> TaxSelectionItems)
    {
        List<Model.Interfaces.ITaxGroupTaxes> taxSkuMaps = new();
        foreach (ITaxSelectionItem taxSKUMap in TaxSelectionItems)
        {
            taxSkuMaps.Add(ConvertToITaxGroupTaxes(taxSKUMap));
        }
        return taxSkuMaps;
    }
    #region Common Util Methods
    private void AddCreateFields(Winit.Modules.Base.Model.IBaseModel baseModel, bool IsUIDRequired)
    {
        baseModel.CreatedBy = _appUser?.Emp?.UID ?? "WINIT";
        baseModel.ModifiedBy = _appUser?.Emp?.UID ?? "WINIT";
        baseModel.CreatedTime = DateTime.Now;
        baseModel.ModifiedTime = DateTime.Now;
        if (IsUIDRequired)
        {
            baseModel.UID = Guid.NewGuid().ToString();
        }
    }
    private void AddUpdateFields(Winit.Modules.Base.Model.IBaseModel baseModel)
    {
        baseModel.ModifiedBy = _appUser?.Emp?.UID ?? "WINIT"; //_appUser.Emp.UID;
        baseModel.ModifiedTime = DateTime.Now;
    }
    #endregion
    #region Abstract Methods
    protected abstract Task<List<ITaxGroup>> GetTaxGroups_Data();
    protected abstract Task<ITaxGroup?> GetTaxGroupByUID_Data(string TaxGroupUID);
    protected abstract Task<List<ITaxSelectionItem>> GetTaxSelectionItems_Data(string TaxGroupUID);
    protected abstract Task<bool> CreateTaxGroupMaster_Data(TaxGroupMasterDTO taxGroupMasterDTO);
    protected abstract Task<bool> UpdateTaxGroupMaster_Data(TaxGroupMasterDTO taxGroupMasterDTO);
    #endregion
}

