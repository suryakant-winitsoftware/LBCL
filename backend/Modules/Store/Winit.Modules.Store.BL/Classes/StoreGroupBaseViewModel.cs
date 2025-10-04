using Winit.Modules.Base.BL.Helper.Interfaces;
using Winit.Modules.Common.BL.Interfaces;
using Winit.Modules.Store.BL.Interfaces;
using Winit.Modules.Store.Model.Classes;
using Winit.Modules.Store.Model.Interfaces;
using Winit.Shared.CommonUtilities.Extensions;
using Winit.Shared.Models.Common;
using Winit.Shared.Models.Enums;

namespace Winit.Modules.Store.BL.Classes;

public abstract class StoreGroupBaseViewModel : IStoreGroupViewModel
{
    // Injection
    private readonly IServiceProvider _serviceProvider;
    private readonly IFilterHelper _filter;
    private readonly ISortHelper _sorter;
    private readonly IListHelper _listHelper;
    private readonly IAppUser _appUser;

    private readonly List<string> _propertiesToSearch = [];
    public List<IStoreGroupItemView> StoreGroupItemViews { get; set; }
    public List<IStoreGroupType> StoreGroupTypes { get; set; }
    public List<FilterCriteria> FilterCriterias { get; set; }

    protected StoreGroupBaseViewModel(
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
        StoreGroupTypes = [];
        StoreGroupItemViews = [];
        FilterCriterias = [];
    }
    public async Task PopulateViewModel()
    {
        StoreGroupTypes = await GetStoreGroupTypes_Data();
        await LoadGrid();
    }
    private async Task LoadGrid()
    {
        StoreGroupItemViews.Clear();
        List<IStoreGroup> storeGroups = await GetStoreGroup_Data(null, 0);
        List<IStoreGroupItemView> convertedstoreGroupItemViews = ConvertToIStoreGroupItemView(storeGroups);
        StoreGroupItemViews.AddRange(convertedstoreGroupItemViews);
    }
    public async Task<bool> CreateStoreGroup(IStoreGroupItemView storeGroupItemView)
    {
        GenerateStoreGroupItemViewFields(storeGroupItemView);
        AddCreateFields(storeGroupItemView, true);
        return await CreateStoreGroup_Data(storeGroupItemView);
    }
    public async Task<bool> DeleteStoreGroup(Winit.Modules.Store.Model.Interfaces.IStoreGroupItemView storeGroupItemView)
    {
        return await DeleteStoreGroup_Data(storeGroupItemView.UID);
    }
    public async Task<bool> UpdateStoreGroup(Winit.Modules.Store.Model.Interfaces.IStoreGroupItemView storeGroupItemView)
    {
        AddUpdateFields(storeGroupItemView);
        return await UpdateStoreGroup_Data(storeGroupItemView);
    }

    public List<IStoreGroupItemView> ConvertToIStoreGroupItemView(List<IStoreGroup> storeGroups, string? ParentUID = null, string? ParentName = null, string? ParentStoreGroupTypeUID = null)
    {
        List<IStoreGroupItemView> storeGroupItemViews = [];
        foreach (IStoreGroup item in storeGroups)
        {
            storeGroupItemViews.Add(ConvertToIStoreGroupItemView(item, ParentUID, ParentName, ParentStoreGroupTypeUID));
        }
        return storeGroupItemViews;
    }

    public IStoreGroupItemView ConvertToIStoreGroupItemView(IStoreGroup storeGroup, string ParentUID, string ParentName, string? ParentStoreGroupTypeUID)
    {
        IStoreGroupItemView storeGroupItemView = new StoreGroupItemView
        {
            Code = storeGroup.Code,
            ItemLevel = storeGroup.ItemLevel,
            Name = storeGroup.Name,
            ParentName = ParentName,
            UID = storeGroup.UID,
            StoreGroupTypeUID = storeGroup.StoreGroupTypeUID,
            ParentStoreGroupTypeUID = ParentStoreGroupTypeUID,
            ParentUID = ParentUID,
            SS = storeGroup.SS,
            CreatedBy = storeGroup.CreatedBy,
            CreatedTime = storeGroup.CreatedTime,
            ModifiedBy = storeGroup.ModifiedBy,
            ModifiedTime = storeGroup.ModifiedTime,
            ServerAddTime = storeGroup.ServerAddTime,
            ServerModifiedTime = storeGroup.ServerModifiedTime,
            HasChild = storeGroup.HasChild,
        };
        if (storeGroup.StoreGroupTypeUID != null)
        {
            storeGroupItemView.StoreGroupTypeName = StoreGroupTypes?.Find(e => e.UID == storeGroup?.StoreGroupTypeUID)?.Name;
            storeGroupItemView.StoreGroupTypeCode = StoreGroupTypes?.Find(e => e.UID == storeGroup?.StoreGroupTypeUID)?.Code;
        }
        return storeGroupItemView;
    }

    public void GenerateStoreGroupItemViewFields(IStoreGroupItemView storeGroupItemView)
    {
        storeGroupItemView.ChildGrids ??= [];
    }

    public async Task GetChildGrid(Winit.Modules.Store.Model.Interfaces.IStoreGroupItemView storeGroupItemView)
    {
        List<IStoreGroup> storeGroups = await GetStoreGroup_Data(storeGroupItemView.UID, storeGroupItemView.ItemLevel + 1);
        storeGroupItemView.ChildGrids = storeGroups != null ? ConvertToIStoreGroupItemView(
            storeGroups, storeGroupItemView.UID, storeGroupItemView.Name, storeGroupItemView.StoreGroupTypeUID) : [];
    }

    public List<ISelectionItem> GetStoreGroupTypeSelectionItems(int Level, bool IsAddItembtn, string? ParentUID = null, bool IsAll = false)
    {
        if (StoreGroupTypes != null)
        {
            return IsAll
                ? StoreGroupTypes.Select(e => new SelectionItem
                {
                    UID = e.UID,
                    Label = e.Name,
                }).ToList<ISelectionItem>()
                : IsAddItembtn
                ? StoreGroupTypes.Where(i => i.ParentUID == null).Select(e => new SelectionItem
                {
                    UID = e.UID,
                    Label = e.Name,
                }).ToList<ISelectionItem>()
                : StoreGroupTypes.Where(i => i.ParentUID == ParentUID && i.LevelNo == Level).Select(e => new SelectionItem
                {
                    UID = e.UID,
                    Label = e.Name,
                }).ToList<ISelectionItem>();
        }
        return [];
    }
    public async Task CreateStoreGroupHierarchy(Winit.Modules.Store.Model.Interfaces.IStoreGroupItemView storeGroupItemView)
    {
        await CreateStoreGroupHierarchyApiCall(storeGroupItemView);
    }

    public Task<IStoreGroupItemView> CreateClone(Winit.Modules.Store.Model.Interfaces.IStoreGroupItemView storeGroupItemView)
    {
        IStoreGroupItemView ClonedstoreGroupItemView = _serviceProvider.CreateInstance<IStoreGroupItemView>();
        ClonedstoreGroupItemView.UID = storeGroupItemView.UID;
        ClonedstoreGroupItemView.SS = storeGroupItemView.SS;
        ClonedstoreGroupItemView.CreatedBy = storeGroupItemView.CreatedBy;
        ClonedstoreGroupItemView.ModifiedBy = storeGroupItemView.ModifiedBy;
        ClonedstoreGroupItemView.CreatedTime = storeGroupItemView.CreatedTime;
        ClonedstoreGroupItemView.ModifiedTime = storeGroupItemView.ModifiedTime;
        ClonedstoreGroupItemView.ServerAddTime = storeGroupItemView.ServerAddTime;
        ClonedstoreGroupItemView.ServerModifiedTime = storeGroupItemView.ServerModifiedTime;
        ClonedstoreGroupItemView.Code = storeGroupItemView.Code;
        ClonedstoreGroupItemView.Name = storeGroupItemView.Name;
        ClonedstoreGroupItemView.ParentUID = storeGroupItemView.ParentUID;
        ClonedstoreGroupItemView.ParentName = storeGroupItemView.ParentName;
        ClonedstoreGroupItemView.StoreGroupTypeUID = storeGroupItemView.StoreGroupTypeUID;
        ClonedstoreGroupItemView.StoreGroupTypeName = storeGroupItemView.StoreGroupTypeName;
        ClonedstoreGroupItemView.IsCreatePopUpOpen = storeGroupItemView.IsCreatePopUpOpen;
        ClonedstoreGroupItemView.IsDeletePopUpOpen = storeGroupItemView.IsDeletePopUpOpen;
        ClonedstoreGroupItemView.IsUpdatePopUpOpen = storeGroupItemView.IsUpdatePopUpOpen;
        ClonedstoreGroupItemView.IsOpen = storeGroupItemView.IsOpen;
        ClonedstoreGroupItemView.ItemLevel = storeGroupItemView.ItemLevel;
        ClonedstoreGroupItemView.ParentStoreGroupTypeUID = storeGroupItemView.ParentStoreGroupTypeUID;
        return Task.Run(() => ClonedstoreGroupItemView);
    }
    public async Task StoreGroupTypeSelectedForParent(Winit.Modules.Store.Model.Interfaces.IStoreGroupItemView Context, string UID)
    {
        await Task.Run(() => { Context.ItemLevel = StoreGroupTypes.Find(e => e.UID == UID).LevelNo; });
    }
    public async Task ApplyFilter(IDictionary<string, string> keyValuePairs)
    {
        FilterCriterias.Clear();
        if (keyValuePairs.TryGetValue("store_group_hierarchy_level_code_name", out string? value) && !string.IsNullOrEmpty(value))
        {
            FilterCriterias.Add(new FilterCriteria("store_group_hierarchy_level_code_name", value, FilterType.Like, filterMode: FilterMode.Or));
        }
        await LoadGrid();
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

    #region Abstarct Methods
    protected abstract Task<List<IStoreGroupType>> GetStoreGroupTypes_Data();
    protected abstract Task<bool> DeleteStoreGroup_Data(string storeGroupUID);
    protected abstract Task<List<IStoreGroup>> GetStoreGroup_Data(string? parentUID, int Level);
    protected abstract Task<bool> UpdateStoreGroup_Data(IStoreGroup storeGroup);
    protected abstract Task<bool> CreateStoreGroup_Data(IStoreGroup storeGroup);
    protected abstract Task CreateStoreGroupHierarchyApiCall(Winit.Modules.Store.Model.Interfaces.IStoreGroupItemView storeGroupItemView);
    #endregion
}
