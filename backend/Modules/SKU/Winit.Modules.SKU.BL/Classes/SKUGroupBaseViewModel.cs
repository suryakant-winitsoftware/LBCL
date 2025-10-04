using Winit.Modules.Base.BL.Helper.Interfaces;
using Winit.Modules.Common.BL.Interfaces;
using Winit.Modules.SKU.BL.Interfaces;
using Winit.Modules.SKU.Model.Interfaces;
using Winit.Modules.SKU.Model.UIInterfaces;
using Winit.Shared.CommonUtilities.Extensions;
using Winit.Shared.Models.Common;
using Winit.Shared.Models.Enums;

namespace Winit.Modules.SKU.BL.Classes;
public abstract class SKUGroupBaseViewModel : ISKUGroupViewModel
{
    // Injection
    private readonly IServiceProvider _serviceProvider;
    private readonly IFilterHelper _filter;
    private readonly ISortHelper _sorter;
    private readonly IListHelper _listHelper;
    private readonly IAppUser _appUser;

    private readonly List<string> _propertiesToSearch = [];
    public List<ISKUGroupItemView> SKUGroupItemViews { get; set; }
    public List<ISKUGroupType> SKUGroupTypes { get; set; }
    public List<ISelectionItem> SupplierSelectionItems { get; set; }
    public List<FilterCriteria> FilterCriterias { get; set; }

    protected SKUGroupBaseViewModel(
            IServiceProvider serviceProvider,
            IFilterHelper filter,
            ISortHelper sorter,
            IListHelper listHelper,
            IAppUser appUser
       )
    {
        _serviceProvider = serviceProvider;
        _filter = filter;
        _sorter = sorter;
        _listHelper = listHelper;
        _appUser = appUser;
        FilterCriterias = [];
        SKUGroupTypes = [];
        SKUGroupItemViews = [];
        SupplierSelectionItems = [];
    }
    public async Task PopulateViewModel()
    {
        SKUGroupTypes = await GetSKUGroupTypes_Data();
        //var supplierSelectionItems = await GetSuppliers_Data();
        //if (supplierSelectionItems != null)
        //{
        //    SupplierSelectionItems = CommonFunctions.ConvertToSelectionItems<Winit.Modules.Org.Model.Interfaces.IOrg>
        //        (supplierSelectionItems, new List<string> { "UID", "Code", "Name" });
        //}
        await LoadGridData();
    }
    private async Task LoadGridData()
    {
        SKUGroupItemViews.Clear();
        List<ISKUGroupItemView> sKUGroups = await GetSKUGroup_Data(null, 0);
        List<ISKUGroupItemView> convertedsKUGroupItemViews = ConvertToISKUGroupItemView(sKUGroups);
        SKUGroupItemViews.AddRange(convertedsKUGroupItemViews);
        //foreach (ISKUGroupItemView item in SKUGroupItemViews)
        //{
        //    List<ISKUGroupItemView> secondLevel = await GetSKUGroup_Data(item.UID, item.ItemLevel + 1);
        //    if (secondLevel != null)
        //    {
        //        item.ChildGrids = [.. ConvertToISKUGroupItemView(secondLevel, item.UID, item.Name, item.SKUGroupTypeUID)];
        //    }
        //}
    }
    public async Task<bool> CreateSKUGroup(ISKUGroupItemView sKUGroupItemView)
    {
        AddCreateFields(sKUGroupItemView, true);
        sKUGroupItemView.ChildGrids ??= [];
        return await CreateSKUGroup_Data(sKUGroupItemView);
    }
    public async Task<bool> DeleteSKUGroup(Winit.Modules.SKU.Model.UIInterfaces.ISKUGroupItemView sKUGroupItemView)
    {
        return await DeleteSKUGroup_Data(sKUGroupItemView.UID);
    }
    public async Task<bool> UpdateSKUGroup(Winit.Modules.SKU.Model.UIInterfaces.ISKUGroupItemView sKUGroupItemView)
    {
        AddUpdateFields(sKUGroupItemView);
        return await UpdateSKUGroup_Data(sKUGroupItemView);
    }
    public List<ISKUGroupItemView> ConvertToISKUGroupItemView(List<ISKUGroupItemView> sKUGroups, string? ParentUID = null, string? ParentName = null, string? ParentSKUGroupTypeUID = null)
    {
        List<ISKUGroupItemView> sKUGroupItemViews = [];
        foreach (ISKUGroupItemView item in sKUGroups)
        {
            sKUGroupItemViews.Add(ConvertToISKUGroupItemView(item, ParentUID, ParentName, ParentSKUGroupTypeUID));
        }
        return sKUGroupItemViews;
    }
    public ISKUGroupItemView ConvertToISKUGroupItemView(ISKUGroupItemView sKUGroupItemView, string ParentUID, string ParentName, string? ParentSKUGroupTypeUID)
    {
        //ISKUGroupItemView sKUGroupItemView = new SKUGroupItemView
        //{
        //    Code = sKUGroup.Code,
        //    ItemLevel = sKUGroup.ItemLevel,
        //    Name = sKUGroup.Name,
        //    ParentName = ParentName,
        //    UID = sKUGroup.UID,
        //    SKUGroupTypeUID = sKUGroup.SKUGroupTypeUID,
        //    ParentSKUGroupTypeUID = ParentSKUGroupTypeUID,
        //    SupplierOrgUID = sKUGroup.SupplierOrgUID,
        //    ParentUID = ParentUID,
        //    SS = sKUGroup.SS,
        //    CreatedBy = sKUGroup.CreatedBy,
        //    CreatedTime = sKUGroup.CreatedTime,
        //    ModifiedBy = sKUGroup.ModifiedBy,
        //    ModifiedTime = sKUGroup.ModifiedTime,
        //    ServerAddTime = sKUGroup.ServerAddTime,
        //    ServerModifiedTime = sKUGroup.ServerModifiedTime,
        //};
        sKUGroupItemView.ParentSKUGroupTypeUID = ParentSKUGroupTypeUID;
        sKUGroupItemView.ParentUID = ParentUID;
        sKUGroupItemView.ParentName = ParentName;
        if (sKUGroupItemView.SKUGroupTypeUID != null)
        {
            sKUGroupItemView.SKUGroupTypeName = SKUGroupTypes?.Find(e => e.UID == sKUGroupItemView?.SKUGroupTypeUID)?.Name;
        }
        if (sKUGroupItemView.SupplierOrgUID != null)
        {
            sKUGroupItemView.SupplierName = SupplierSelectionItems.Find(e => e.UID == sKUGroupItemView.SupplierOrgUID)?.Label;
        }
        return sKUGroupItemView;
    }
    public async Task getChildGrid(Winit.Modules.SKU.Model.UIInterfaces.ISKUGroupItemView sKUGroupItemView)
    {
        if (sKUGroupItemView != null /*&& sKUGroupItemView.ChildGrids != null && sKUGroupItemView.ChildGrids.Any()*/)
        {
            //foreach (Winit.Modules.SKU.Model.UIInterfaces.ISKUGroupItemView item in sKUGroupItemView.ChildGrids)
            //{
            List<ISKUGroupItemView> sKUGroups = await GetSKUGroup_Data(sKUGroupItemView.UID, sKUGroupItemView.ItemLevel + 1);
            sKUGroupItemView.ChildGrids = sKUGroups != null ? ConvertToISKUGroupItemView(sKUGroups, sKUGroupItemView.UID, sKUGroupItemView.Name, sKUGroupItemView.SKUGroupTypeUID) : null;
            //}
        }
    }
    public async Task<List<ISelectionItem>> GetSKuGroupTypeSelectionItems(int Level, bool IsAddItembtn, string? ParentUID = null, bool IsAll = false)
    {
        if (SKUGroupTypes != null)
        {
            return IsAll
                ? SKUGroupTypes.Select(e => new SelectionItem
                {
                    UID = e.UID,
                    Label = e.Name,
                    Code = e.Code
                }).ToList<ISelectionItem>()
                : IsAddItembtn
                ? SKUGroupTypes.Where(i => i.ParentUID == null).Select(e => new SelectionItem
                {
                    UID = e.UID,
                    Label = e.Name,
                    Code = e.Code
                }).ToList<ISelectionItem>()
                : SKUGroupTypes.Where(i => i.ParentUID == ParentUID && i.ItemLevel == Level).Select(e => new SelectionItem
                {
                    UID = e.UID,
                    Label = e.Name,
                    Code = e.Code
                }).ToList<ISelectionItem>();
        }
        await Task.CompletedTask;
        return [];
    }
    public Task<ISKUGroupItemView> CreateClone(Winit.Modules.SKU.Model.UIInterfaces.ISKUGroupItemView sKUGroupItemView)
    {
        ISKUGroupItemView ClonedsKUGroupItemView = _serviceProvider.CreateInstance<ISKUGroupItemView>();
        ClonedsKUGroupItemView.UID = sKUGroupItemView.UID;
        ClonedsKUGroupItemView.SS = sKUGroupItemView.SS;
        ClonedsKUGroupItemView.CreatedBy = sKUGroupItemView.CreatedBy;
        ClonedsKUGroupItemView.ModifiedBy = sKUGroupItemView.ModifiedBy;
        ClonedsKUGroupItemView.CreatedTime = sKUGroupItemView.CreatedTime;
        ClonedsKUGroupItemView.ModifiedTime = sKUGroupItemView.ModifiedTime;
        ClonedsKUGroupItemView.ServerAddTime = sKUGroupItemView.ServerAddTime;
        ClonedsKUGroupItemView.ServerModifiedTime = sKUGroupItemView.ServerModifiedTime;
        ClonedsKUGroupItemView.Code = sKUGroupItemView.Code;
        ClonedsKUGroupItemView.Name = sKUGroupItemView.Name;
        ClonedsKUGroupItemView.ParentUID = sKUGroupItemView.ParentUID;
        ClonedsKUGroupItemView.ParentName = sKUGroupItemView.ParentName;
        ClonedsKUGroupItemView.SupplierOrgUID = sKUGroupItemView.SupplierOrgUID;
        ClonedsKUGroupItemView.SupplierName = sKUGroupItemView.SupplierName;
        ClonedsKUGroupItemView.SKUGroupTypeUID = sKUGroupItemView.SKUGroupTypeUID;
        ClonedsKUGroupItemView.SKUGroupTypeName = sKUGroupItemView.SKUGroupTypeName;
        ClonedsKUGroupItemView.IsCreatePopUpOpen = sKUGroupItemView.IsCreatePopUpOpen;
        ClonedsKUGroupItemView.IsDeletePopUpOpen = sKUGroupItemView.IsDeletePopUpOpen;
        ClonedsKUGroupItemView.IsUpdatePopUpOpen = sKUGroupItemView.IsUpdatePopUpOpen;
        ClonedsKUGroupItemView.IsOpen = sKUGroupItemView.IsOpen;
        ClonedsKUGroupItemView.ItemLevel = sKUGroupItemView.ItemLevel;
        ClonedsKUGroupItemView.ParentSKUGroupTypeUID = sKUGroupItemView.ParentSKUGroupTypeUID;
        return Task.Run(() => ClonedsKUGroupItemView);
    }
    public async Task SKUGroupTypeSelectedForParent(Winit.Modules.SKU.Model.UIInterfaces.ISKUGroupItemView Context, string UID)
    {
        await Task.Run(() => { Context.ItemLevel = SKUGroupTypes.Find(e => e.UID == UID).ItemLevel; });
    }
    public async Task ApplyFilter(IDictionary<string, string> keyValuePairs)
    {
        FilterCriterias.Clear();
        if (keyValuePairs.TryGetValue("product_hierarchylevel_code_name", out string? value) && !string.IsNullOrEmpty(value))
        {
            FilterCriterias.Add(new FilterCriteria("product_hierarchylevel_code_name", value, FilterType.Like, filterMode: FilterMode.Or));
        }
        await LoadGridData();
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
    public async Task CreateSKUHierarchy(Winit.Modules.SKU.Model.UIInterfaces.ISKUGroupItemView sKUGroupItemView)
    {
        await CreateSKUHierarchyAPICall(sKUGroupItemView);
    }
    #endregion
    #region Abstarct Methods
    protected abstract Task<List<ISKUGroupType>> GetSKUGroupTypes_Data();
    protected abstract Task<List<ISKUGroupItemView>> GetSKUGroup_Data(string? ParentUID, int level);
    protected abstract Task<List<Winit.Modules.Org.Model.Interfaces.IOrg>> GetSuppliers_Data();
    protected abstract Task<bool> UpdateSKUGroup_Data(ISKUGroup sKUGroup);
    protected abstract Task<bool> CreateSKUGroup_Data(ISKUGroup sKUGroup);
    protected abstract Task<bool> DeleteSKUGroup_Data(string sKUGroupUID);
    protected abstract Task CreateSKUHierarchyAPICall(Winit.Modules.SKU.Model.UIInterfaces.ISKUGroupItemView sKUGroupItemView);

    #endregion
}


