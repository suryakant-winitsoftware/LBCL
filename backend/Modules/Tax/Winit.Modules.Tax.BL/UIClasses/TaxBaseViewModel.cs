using Winit.Modules.Base.BL.Helper.Interfaces;
using Winit.Modules.Common.BL.Interfaces;
using Winit.Modules.Common.Model.Interfaces;
using Winit.Modules.Setting.BL.Interfaces;
using Winit.Modules.SKU.Model.Interfaces;
using Winit.Modules.Tax.BL.UIInterfaces;
using Winit.Modules.Tax.Model.Classes;
using Winit.Modules.Tax.Model.Interfaces;
using Winit.Modules.Tax.Model.UIInterfaces;
using Winit.Shared.CommonUtilities.Extensions;
using Winit.Shared.Models.Common;
using Winit.Shared.Models.Enums;

namespace Winit.Modules.Tax.BL.UIClasses;

public abstract class TaxBaseViewModel : ITaxViewModel
{
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; }
    public int TotalTaxItemsCount { get; set; }
    public List<ITaxItemView> TaxItemViews { get; set; }
    public ITax SelectedTax { get; set; }
    public List<Winit.Modules.Tax.Model.Interfaces.ITaxSkuMap> TaxSKuMapListRaw { get; set; }
    public List<ISKUMaster> SKUMasterList { get; set; }
    public List<FilterCriteria> TaxFilterCriterias { get; set; }
    public List<SortCriteria> TaxSortCriterials { get; set; }
    public List<ISKU> SKUList { get; set; }
    public List<ISKUAttributes> SkuAttributesList { get; set; }
    public List<ITaxSKUMapItemView> TaxSKUMapItemViews { get; set; }

    // Injection
    private readonly IServiceProvider _serviceProvider;
    private readonly IFilterHelper _filter;
    private readonly ISortHelper _sorter;
    private readonly List<string> _propertiesToSearch = new();
    private readonly IListHelper _listHelper;
    private readonly IAppUser _appUser;
    private readonly IAppSetting _appSetting;
    private readonly IDataManager _dataManager;


    protected TaxBaseViewModel(IServiceProvider serviceProvider,
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
        TaxItemViews = new List<ITaxItemView>();
        TaxSortCriterials = new List<SortCriteria>();
        TaxFilterCriterias = new List<FilterCriteria>();
        SkuAttributesList = new List<ISKUAttributes>();
        TaxSKUMapItemViews = new List<ITaxSKUMapItemView>();
        SKUList = new List<ISKU>();
        SKUMasterList = new List<ISKUMaster>();
        TaxSKuMapListRaw = new List<Model.Interfaces.ITaxSkuMap>();
        // Property set for Search
        _propertiesToSearch.Add("Code");
        _propertiesToSearch.Add("Name");
    }
    public async Task PopulateViewModel()
    {
        List<ITax> data = await GetTaxs_Data();
        if (data != null)
        {
            TaxItemViews.Clear();
            TaxItemViews.AddRange(ConvertToTaxItemView(data));
        }
    }
    public async Task PopulateAddEditTaxPage()
    {
        SKUMasterList = await GetSKUMasterData_Data();
        SkuAttributesList.AddRange(SKUMasterList.SelectMany(e => e.SKUAttributes.ToList()).Where(attr => attr != null));
        SKUList.AddRange(SKUMasterList.Select(e => e.SKU).Where(sku => sku != null));
    }

    /// <summary>
    /// This will seach data from TaxItemViews and store in FilteredTaxItemViews & DisplayedTaxItemViews
    /// </summary>
    /// <param name="filterCriterias"></param>
    /// <param name="filterMode"></param>
    /// <returns></returns>
    /// 

    public async Task ApplyFilter(List<Shared.Models.Enums.FilterCriteria> filterCriterias)
    {
        TaxFilterCriterias.Clear();
        TaxFilterCriterias.AddRange(filterCriterias);
        await PopulateViewModel();
    }
    public async Task ResetFilter()
    {
        TaxFilterCriterias.Clear();
        await PopulateViewModel();
    }
    /// <summary>
    /// This will sort data from FilteredTaxItemViews and store in FilteredTaxItemViews & DisplayedTaxItemViews
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
    public async Task GetExistingTaxWithTaxSkuMaps(string TaxUID)
    {
        TaxSKUMapItemViews.Clear();
        TaxMasterDTO? taxMaster = await GetTaxMaster_Data(TaxUID);
        if (taxMaster != null && taxMaster.Tax != null)
        {
            SelectedTax = taxMaster.Tax;
            if (taxMaster.TaxSKUMapList != null && taxMaster.TaxSKUMapList.Any())
            {
                TaxSKuMapListRaw.Clear();
                TaxSKuMapListRaw.AddRange(taxMaster.TaxSKUMapList.OfType<Winit.Modules.Tax.Model.Interfaces.ITaxSkuMap>().ToList());
                IEnumerable<string> skuUIDs = TaxSKuMapListRaw.Select(i => i.SKUUID);
                if (skuUIDs != null)
                {
                    List<ISKU> sKus = SKUList.FindAll(e => skuUIDs.Contains(e.UID));
                    TaxSKUMapItemViews.AddRange(ConvertToTaxSKuMapItemView(sKus, SelectedTax.UID, ActionType.Update));
                }
            }
        }
    }
    public async Task OnAddSKuSelectedItems(List<ISelectionItem> selectionItems, string taxUID, ActionType actionType)
    {
        List<ISKU> SelectedsKUs = SKUList.FindAll(e => selectionItems.Select(i => i.UID).Contains(e.UID));
        TaxSKUMapItemViews.AddRange(ConvertToTaxSKuMapItemView(SelectedsKUs, taxUID, actionType));
    }
    public async Task<bool> CreateTaxMaster(ITax tax)
    {
        AddCreateFields(tax, false);
        Winit.Modules.Tax.Model.Interfaces.ITaxMaster taxMaster = new Model.Classes.TaxMaster
        {
            Tax = tax,
            TaxSkuMapList = ConvertToITaxSKUMap(TaxSKUMapItemViews)
        };
        return await CreateTaxMaster_Data(taxMaster);
    }

    public async Task<bool> UpdateTaxMaster(ITax tax)
    {
        AddUpdateFields(tax);
        Winit.Modules.Tax.Model.Interfaces.ITaxMaster taxMaster = new Model.Classes.TaxMaster
        {
            Tax = tax,
            TaxSkuMapList = ConvertToITaxSKUMap(TaxSKUMapItemViews.FindAll(e => e.ActionType != ActionType.Update))
        };
        return await UpdateTaxMaster_Data(taxMaster);
    }
    public ITax GetTax()
    {
        return SelectedTax;
    }
    //ModelConversionMethods
    private List<ITaxItemView> ConvertToTaxItemView(List<ITax> taxs)
    {
        List<ITaxItemView> taxItemViews = new();
        foreach (ITax tax in taxs)
        {
            taxItemViews.Add(ConvertToTaxItemView(tax));
        }
        return taxItemViews;
    }
    private ITaxItemView ConvertToTaxItemView(ITax tax)
    {
        ITaxItemView taxItemView = _serviceProvider.CreateInstance<ITaxItemView>();
        taxItemView.ApplicableAt = tax.ApplicableAt;
        taxItemView.Name = tax.Name;
        taxItemView.Status = tax.Status;
        taxItemView.ValidFrom = tax.ValidFrom;
        taxItemView.ValidTo = tax.ValidUpto;
        taxItemView.CalculationType = tax.TaxCalculationType;
        taxItemView.BaseTaxRate = tax.BaseTaxRate;
        taxItemView.UID = tax.UID;
        return taxItemView;
    }
    private List<ITaxSKUMapItemView> ConvertToTaxSKuMapItemView(List<ISKU> sKUs, string taxUID, ActionType actionType)
    {
        List<ITaxSKUMapItemView> taxSKUMapItemViews = new();
        foreach (ISKU sKU in sKUs)
        {
            taxSKUMapItemViews.Add(ConvertToTaxSKuMapItemView(sKU, taxUID, actionType));
        }
        return taxSKUMapItemViews;
    }
    private ITaxSKUMapItemView ConvertToTaxSKuMapItemView(ISKU sKU, string taxUID, ActionType actionType)
    {
        ITaxSKUMapItemView taxSKUMapItemView = _serviceProvider.CreateInstance<ITaxSKUMapItemView>();
        taxSKUMapItemView.SKUName = sKU.Name;
        taxSKUMapItemView.SKUCode = sKU.Code;
        taxSKUMapItemView.SKUUID = sKU.UID;
        taxSKUMapItemView.CompanyUID = sKU.CompanyUID;
        taxSKUMapItemView.TaxUID = taxUID;
        taxSKUMapItemView.ActionType = actionType;
        return taxSKUMapItemView;
    }
    private Model.Interfaces.ITaxSkuMap ConvertToITaxSKUMap(ITaxSKUMapItemView taxSKUMapItemView)
    {
        Model.Interfaces.ITaxSkuMap taxSkuMap = _serviceProvider.CreateInstance<Model.Interfaces.ITaxSkuMap>();
        taxSkuMap.TaxUID = taxSKUMapItemView.TaxUID;
        taxSkuMap.SKUUID = taxSKUMapItemView.SKUUID;
        taxSkuMap.ActionType = taxSKUMapItemView.ActionType;
        if (taxSkuMap.ActionType == ActionType.Delete)
        {
            taxSkuMap.UID = TaxSKuMapListRaw.Find(e => e.SKUUID.Equals(taxSkuMap.SKUUID))?.UID;
            AddUpdateFields(taxSkuMap);
        }
        else
        {
            AddCreateFields(taxSkuMap, true);
        }
        return taxSkuMap;
    }
    private List<Model.Interfaces.ITaxSkuMap> ConvertToITaxSKUMap(List<ITaxSKUMapItemView> taxSKUMapItemViews)
    {
        List<Model.Interfaces.ITaxSkuMap> taxSkuMaps = new();
        foreach (ITaxSKUMapItemView taxSKUMap in taxSKUMapItemViews)
        {
            taxSkuMaps.Add(ConvertToITaxSKUMap(taxSKUMap));
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
    protected abstract Task<bool> UpdateTaxMaster_Data(Winit.Modules.Tax.Model.Interfaces.ITaxMaster taxMaster);
    protected abstract Task<bool> CreateTaxMaster_Data(Winit.Modules.Tax.Model.Interfaces.ITaxMaster taxMaster);
    protected abstract Task<TaxMasterDTO?> GetTaxMaster_Data(string TaxUID);
    protected abstract Task<List<ISKUMaster>> GetSKUMasterData_Data();
    protected abstract Task<List<ITax>> GetTaxs_Data();

    #endregion

}

