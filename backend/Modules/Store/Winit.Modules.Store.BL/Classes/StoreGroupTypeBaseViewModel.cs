using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Winit.Modules.Base.BL.Helper.Interfaces;
using Winit.Modules.Common.BL.Interfaces;
using Winit.Modules.Store.BL.Interfaces;
using Winit.Modules.Store.Model.Classes;
using Winit.Modules.Store.Model.Interfaces;
using Winit.Shared.CommonUtilities.Extensions;
using Winit.Shared.Models.Common;
using Winit.Shared.Models.Enums;

namespace Winit.Modules.Store.BL.Classes;

public abstract class StoreGroupTypeBaseViewModel : IStoreGroupTypeViewModel
{
    private IServiceProvider _serviceProvider;
    private readonly IFilterHelper _filter;
    private readonly ISortHelper _sorter;
    private readonly IListHelper _listHelper;
    private readonly IAppUser _appUser;
    private List<string> _propertiesToSearch = new List<string>();
    public List<IStoreGroupTypeItemView> StoreGroupTypeItemViews { get; set; }
    public List<FilterCriteria> FilterCriterias { get; set; }

    public StoreGroupTypeBaseViewModel(
        IServiceProvider serviceProvider,
            IFilterHelper filter,
            ISortHelper sorter,
            IListHelper listHelper, IAppUser appUser)
    {
        _serviceProvider = serviceProvider;
        _filter = filter;
        _sorter = sorter;
        _listHelper = listHelper;
        _appUser = appUser;
        FilterCriterias = new List<FilterCriteria>();
        StoreGroupTypeItemViews = new List<IStoreGroupTypeItemView>();
    }
    public async Task PopulateViewModel()
    {
        StoreGroupTypeItemViews.Clear();
        List<IStoreGroupType> storeGroupTypes = await GetStoreGroupType_Data(null, 0);
        if (storeGroupTypes != null && storeGroupTypes.Any())
        {
            List<IStoreGroupTypeItemView> convertedstoreGroupTypeItemViews = ConvertToIStoreGroupTypeItemView(storeGroupTypes);
            StoreGroupTypeItemViews.AddRange(convertedstoreGroupTypeItemViews);
        }
        foreach (var item in StoreGroupTypeItemViews)
        {
            var secondLevel = await GetStoreGroupType_Data(item.UID, item.LevelNo + 1);
            if (secondLevel != null)
            {
                item.ChildGrids = new List<IStoreGroupTypeItemView>();
                item.ChildGrids.AddRange(ConvertToIStoreGroupTypeItemView(secondLevel, item.UID, item.Name));
            }
        }
    }
    public async Task<bool> CreateStoreGroupType(IStoreGroupTypeItemView storeGroupTypeItemView)
    {
        await GenerateStoreGroupTypeItemViewFields(storeGroupTypeItemView);
        return await CreateStoreGroup_Data(storeGroupTypeItemView);
    }
    public async Task<bool> DeleteStoreGroupType(Winit.Modules.Store.Model.Interfaces.IStoreGroupTypeItemView storeGroupTypeItemView)
    {
        return await DeleteStoreGroupType_Data(storeGroupTypeItemView.UID);
    }
    public async Task<bool> UpdateStoreGroupType(Winit.Modules.Store.Model.Interfaces.IStoreGroupTypeItemView storeGroupTypeItemView)
    {
        AddUpdateFields(storeGroupTypeItemView);
        return await UpdateStoreGroupType_Data(storeGroupTypeItemView);
    }
    public List<IStoreGroupTypeItemView> ConvertToIStoreGroupTypeItemView(List<IStoreGroupType> storeGroupTypes, string ParentUID = null, string ParentName = null, string? ParentStoreGroupTypeTypeUID = null)
    {
        List<IStoreGroupTypeItemView> storeGroupTypeItemViews = new List<IStoreGroupTypeItemView>();
        foreach (IStoreGroupType item in storeGroupTypes)
        {
            storeGroupTypeItemViews.Add(ConvertToIStoreGroupTypeItemView(item, ParentUID, ParentName, ParentStoreGroupTypeTypeUID));
        }
        return storeGroupTypeItemViews;
    }
    public IStoreGroupTypeItemView ConvertToIStoreGroupTypeItemView(IStoreGroupType storeGroupType, string ParentUID, string ParentName, string? ParentStoreGroupTypeTypeUID)
    {
        IStoreGroupTypeItemView storeGroupTypeItemView = new StoreGroupTypeItemView
        {
            Code = storeGroupType.Code,
            LevelNo = storeGroupType.LevelNo,
            Name = storeGroupType.Name,
            ParentName = ParentName,
            UID = storeGroupType.UID,
            ParentUID = ParentUID,
            SS = storeGroupType.SS,
            CreatedBy = storeGroupType.CreatedBy,
            CreatedTime = storeGroupType.CreatedTime,
            ModifiedBy = storeGroupType.ModifiedBy,
            ModifiedTime = storeGroupType.ModifiedTime,
            ServerAddTime = storeGroupType.ServerAddTime,
            ServerModifiedTime = storeGroupType.ServerModifiedTime,
        };
        return storeGroupTypeItemView;
    }
    public async Task GenerateStoreGroupTypeItemViewFields(IStoreGroupTypeItemView storeGroupTypeItemView)
    {
        AddCreateFields(storeGroupTypeItemView, true);
        if (storeGroupTypeItemView.ChildGrids == null) storeGroupTypeItemView.ChildGrids = new List<IStoreGroupTypeItemView>();
        await Task.CompletedTask;
    }
    public async Task getChildGrid(Winit.Modules.Store.Model.Interfaces.IStoreGroupTypeItemView storeGroupTypeItemView)
    {
        foreach (Winit.Modules.Store.Model.Interfaces.IStoreGroupTypeItemView item in storeGroupTypeItemView.ChildGrids)
        {
            List<IStoreGroupType> storeGroupTypes = await GetStoreGroupType_Data(item.UID, item.LevelNo + 1);
            //item.ChildGrids = new List<IStoreGroupTypeItemView>();
            item.ChildGrids = storeGroupTypes != null ? ConvertToIStoreGroupTypeItemView(storeGroupTypes, item.UID, item.Name) : new();
        }
    }
    public Task<IStoreGroupTypeItemView> CreateClone(Winit.Modules.Store.Model.Interfaces.IStoreGroupTypeItemView storeGroupTypeItemView)
    {
        IStoreGroupTypeItemView ClonedstoreGroupTypeItemView = _serviceProvider.CreateInstance<IStoreGroupTypeItemView>();
        ClonedstoreGroupTypeItemView.UID = storeGroupTypeItemView.UID;
        ClonedstoreGroupTypeItemView.SS = storeGroupTypeItemView.SS;
        ClonedstoreGroupTypeItemView.CreatedBy = storeGroupTypeItemView.CreatedBy;
        ClonedstoreGroupTypeItemView.ModifiedBy = storeGroupTypeItemView.ModifiedBy;
        ClonedstoreGroupTypeItemView.CreatedTime = storeGroupTypeItemView.CreatedTime;
        ClonedstoreGroupTypeItemView.ModifiedTime = storeGroupTypeItemView.ModifiedTime;
        ClonedstoreGroupTypeItemView.ServerAddTime = storeGroupTypeItemView.ServerAddTime;
        ClonedstoreGroupTypeItemView.ServerModifiedTime = storeGroupTypeItemView.ServerModifiedTime;
        ClonedstoreGroupTypeItemView.Code = storeGroupTypeItemView.Code;
        ClonedstoreGroupTypeItemView.Name = storeGroupTypeItemView.Name;
        ClonedstoreGroupTypeItemView.ParentUID = storeGroupTypeItemView.ParentUID;
        ClonedstoreGroupTypeItemView.ParentName = storeGroupTypeItemView.ParentName;
        ClonedstoreGroupTypeItemView.IsCreatePopUpOpen = storeGroupTypeItemView.IsCreatePopUpOpen;
        ClonedstoreGroupTypeItemView.IsDeletePopUpOpen = storeGroupTypeItemView.IsDeletePopUpOpen;
        ClonedstoreGroupTypeItemView.IsUpdatePopUpOpen = storeGroupTypeItemView.IsUpdatePopUpOpen;
        ClonedstoreGroupTypeItemView.IsOpen = storeGroupTypeItemView.IsOpen;
        ClonedstoreGroupTypeItemView.LevelNo = storeGroupTypeItemView.LevelNo;
        return Task.Run(() => ClonedstoreGroupTypeItemView);
    }
    public async Task ApplyFilter(IDictionary<string, string> keyValuePairs)
    {
        FilterCriterias.Clear();
        string? value;
        if (keyValuePairs.TryGetValue("customer_group_type_hierarchy_code_name", out value) && !string.IsNullOrEmpty(value))
        {
            FilterCriterias.Add(new FilterCriteria("customer_group_type_hierarchy_code_name", value, FilterType.Like, filterMode: FilterMode.Or));
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
    #region Abstarct Methods
    protected abstract Task<bool> CreateStoreGroup_Data(IStoreGroupType storeGroupType);
    protected abstract Task<bool> DeleteStoreGroupType_Data(string storeGropTypeUID);
    protected abstract Task<bool> UpdateStoreGroupType_Data(IStoreGroupType storeGroupType);
    protected abstract Task<List<IStoreGroupType>> GetStoreGroupType_Data(string? parentUID, int itemLevel);
    #endregion
}

