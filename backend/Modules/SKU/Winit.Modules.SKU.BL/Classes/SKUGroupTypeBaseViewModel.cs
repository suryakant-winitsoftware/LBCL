using Winit.Modules.SKU.BL.Interfaces;
using Winit.Modules.SKU.Model.Interfaces;
using Winit.Modules.SKU.Model.Classes;
using Winit.Shared.CommonUtilities.Extensions;
using Winit.Modules.Base.BL.Helper.Interfaces;
using Winit.Shared.Models.Enums;
using Winit.Modules.Common.BL.Interfaces;

namespace Winit.Modules.SKU.BL.Classes;
public abstract class SKUGroupTypeBaseViewModel : ISKUGroupTypeViewModel
{
    // Injection
    private IServiceProvider _serviceProvider;
    private readonly IFilterHelper _filter;
    private readonly ISortHelper _sorter;
    private readonly IListHelper _listHelper;
    private readonly IAppUser _appUser;

    private List<string> _propertiesToSearch = new List<string>();
    public List<ISKUGroupTypeItemView> SKUGroupTypeItemViews { get; set; }
    public List<FilterCriteria> FilterCriterias { get; set; }

    public SKUGroupTypeBaseViewModel(
            IServiceProvider serviceProvider,
            IFilterHelper filter,
            ISortHelper sorter,
            IListHelper listHelper,
            IAppUser appUser)
    {
        _serviceProvider = serviceProvider;
        _filter = filter;
        _sorter = sorter;
        _listHelper = listHelper;
        _appUser = appUser;
        SKUGroupTypeItemViews = new List<ISKUGroupTypeItemView>();
        FilterCriterias = new List<FilterCriteria>();
    }
    public async Task PopulateViewModel()
    {
        SKUGroupTypeItemViews.Clear();
        // Fix: Get root level items (level 1 with parent null) - matching the actual data structure
        List<ISKUGroupType> sKUGroupTypes = await GetSKUGroupType_Data(null, 1);
        if (sKUGroupTypes != null && sKUGroupTypes.Any())
        {
            List<ISKUGroupTypeItemView> convertedsKUGroupTypeItemViews = ConvertToISKUGroupTypeItemView(sKUGroupTypes);
            SKUGroupTypeItemViews.AddRange(convertedsKUGroupTypeItemViews);
        }
        FilterCriterias.Clear();
        await LoadGrid();
    }
    private async Task LoadGrid()
    {
        foreach (var item in SKUGroupTypeItemViews)
        {
            var secondLevel = await GetSKUGroupType_Data(item.UID, item.ItemLevel + 1);
            if (secondLevel != null)
            {
                item.ChildGrids = new List<ISKUGroupTypeItemView>();
                item.ChildGrids.AddRange(ConvertToISKUGroupTypeItemView(secondLevel, item.UID, item.Name));
            }
        }
    }
    public async Task<bool> CreateSKUGroupType(ISKUGroupTypeItemView sKUGroupTypeItemView)
    {
        if (sKUGroupTypeItemView.ChildGrids == null) sKUGroupTypeItemView.ChildGrids = new List<ISKUGroupTypeItemView>();
        AddCreateFields(sKUGroupTypeItemView,true);
        return await CreateSKUGroup_Data(sKUGroupTypeItemView);
    }
    public async Task<bool> DeleteSKUGroupType(Winit.Modules.SKU.Model.Interfaces.ISKUGroupTypeItemView sKUGroupTypeItemView)
    {
        return await DeleteSKUGroupType_Data(sKUGroupTypeItemView.UID);
    }
    public async Task<bool> UpdateSKUGroupType(Winit.Modules.SKU.Model.Interfaces.ISKUGroupTypeItemView sKUGroupTypeItemView)
    {
        AddUpdateFields(sKUGroupTypeItemView);
        return await UpdateSKUGroupType_Data(sKUGroupTypeItemView);
    }
    public List<ISKUGroupTypeItemView> ConvertToISKUGroupTypeItemView(List<ISKUGroupType> sKUGroupTypes, string ParentUID = null, string ParentName = null, string? ParentSKUGroupTypeTypeUID = null)
    {
        List<ISKUGroupTypeItemView> sKUGroupTypeItemViews = new List<ISKUGroupTypeItemView>();
        foreach (ISKUGroupType item in sKUGroupTypes)
        {
            sKUGroupTypeItemViews.Add(ConvertToISKUGroupTypeItemView(item, ParentUID, ParentName, ParentSKUGroupTypeTypeUID));
        }
        return sKUGroupTypeItemViews;
    }
    public ISKUGroupTypeItemView ConvertToISKUGroupTypeItemView(ISKUGroupType sKUGroupType, string ParentUID, string ParentName, string? ParentSKUGroupTypeTypeUID)
    {
        ISKUGroupTypeItemView sKUGroupTypeItemView = new SKUGroupTypeItemView
        {
            Code = sKUGroupType.Code,
            ItemLevel = sKUGroupType.ItemLevel,
            Name = sKUGroupType.Name,
            ParentName = ParentName,
            UID = sKUGroupType.UID,
            ParentUID = ParentUID,
            SS = sKUGroupType.SS,
            CreatedBy = sKUGroupType.CreatedBy,
            CreatedTime = sKUGroupType.CreatedTime,
            ModifiedBy = sKUGroupType.ModifiedBy,
            ModifiedTime = sKUGroupType.ModifiedTime,
            ServerAddTime = sKUGroupType.ServerAddTime,
            ServerModifiedTime = sKUGroupType.ServerModifiedTime,
        };
        return sKUGroupTypeItemView;
    }
    public async Task GetChildGrid(Winit.Modules.SKU.Model.Interfaces.ISKUGroupTypeItemView sKUGroupTypeItemView)
    {
        foreach (Winit.Modules.SKU.Model.Interfaces.ISKUGroupTypeItemView item in sKUGroupTypeItemView.ChildGrids)
        {
            List<ISKUGroupType> sKUGroupTypes = await GetSKUGroupType_Data(item.UID, item.ItemLevel + 1);
            //item.ChildGrids = new List<ISKUGroupTypeItemView>();
            item.ChildGrids = sKUGroupTypes != null ?
                ConvertToISKUGroupTypeItemView(sKUGroupTypes, item.UID, item.Name) : new List<ISKUGroupTypeItemView>();
        }
    }
    public Task<ISKUGroupTypeItemView> CreateClone(Winit.Modules.SKU.Model.Interfaces.ISKUGroupTypeItemView sKUGroupTypeItemView)
    {
        ISKUGroupTypeItemView ClonedsKUGroupTypeItemView = _serviceProvider.CreateInstance<ISKUGroupTypeItemView>();
        ClonedsKUGroupTypeItemView.UID = sKUGroupTypeItemView.UID;
        ClonedsKUGroupTypeItemView.SS = sKUGroupTypeItemView.SS;
        ClonedsKUGroupTypeItemView.CreatedBy = sKUGroupTypeItemView.CreatedBy;
        ClonedsKUGroupTypeItemView.ModifiedBy = sKUGroupTypeItemView.ModifiedBy;
        ClonedsKUGroupTypeItemView.CreatedTime = sKUGroupTypeItemView.CreatedTime;
        ClonedsKUGroupTypeItemView.ModifiedTime = sKUGroupTypeItemView.ModifiedTime;
        ClonedsKUGroupTypeItemView.ServerAddTime = sKUGroupTypeItemView.ServerAddTime;
        ClonedsKUGroupTypeItemView.ServerModifiedTime = sKUGroupTypeItemView.ServerModifiedTime;
        ClonedsKUGroupTypeItemView.Code = sKUGroupTypeItemView.Code;
        ClonedsKUGroupTypeItemView.Name = sKUGroupTypeItemView.Name;
        ClonedsKUGroupTypeItemView.ParentUID = sKUGroupTypeItemView.ParentUID;
        ClonedsKUGroupTypeItemView.ParentName = sKUGroupTypeItemView.ParentName;
        ClonedsKUGroupTypeItemView.IsCreatePopUpOpen = sKUGroupTypeItemView.IsCreatePopUpOpen;
        ClonedsKUGroupTypeItemView.IsDeletePopUpOpen = sKUGroupTypeItemView.IsDeletePopUpOpen;
        ClonedsKUGroupTypeItemView.IsUpdatePopUpOpen = sKUGroupTypeItemView.IsUpdatePopUpOpen;
        ClonedsKUGroupTypeItemView.IsOpen = sKUGroupTypeItemView.IsOpen;
        ClonedsKUGroupTypeItemView.ItemLevel = sKUGroupTypeItemView.ItemLevel;
        return Task.Run(() => ClonedsKUGroupTypeItemView);
    }
    public async Task ApplyFilter(IDictionary<string, string> keyValuePairs)
    {
        FilterCriterias.Clear();
        string? value;
        if (keyValuePairs.TryGetValue("sku_group_type_hierarchy_level_code_name", out value) && !string.IsNullOrEmpty(value))
        {
            FilterCriterias.Add(new FilterCriteria("sku_group_type_hierarchy_level_code_name", value, FilterType.Like, filterMode: FilterMode.Or));
        }
        await PopulateViewModel();
    }
    #region Common Util Methods
    private void AddCreateFields(Winit.Modules.Base.Model.IBaseModel baseModel, bool IsUIDRequired)
    {
        baseModel.CreatedBy = _appUser?.Emp?.UID ?? "WINIT";
        baseModel.ModifiedBy = _appUser?.Emp?.UID ?? "WINIT";
        baseModel.CreatedTime = DateTime.Now;
        baseModel.ModifiedTime = DateTime.Now;
        if (IsUIDRequired) baseModel.UID = Guid.NewGuid().ToString();
    }
    private void AddUpdateFields(Winit.Modules.Base.Model.IBaseModel baseModel)
    {
        baseModel.ModifiedBy = _appUser?.Emp?.UID ?? "WINIT"; //_appUser.Emp.UID;
        baseModel.ModifiedTime = DateTime.Now;
    }
    #endregion

    #region Abstract Methods
    protected abstract Task<List<ISKUGroupType>> GetSKUGroupType_Data(string parentUID, int level);
    protected abstract Task<bool> UpdateSKUGroupType_Data(ISKUGroupType sKUGroupType);
    protected abstract Task<bool> DeleteSKUGroupType_Data(string sKUGroupTypeUID);
    protected abstract Task<bool> CreateSKUGroup_Data(ISKUGroupType sKUGroupType);
    #endregion
}

