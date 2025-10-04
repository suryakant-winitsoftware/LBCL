using Winit.Modules.Base.BL.Helper.Interfaces;
using Winit.Modules.Common.BL.Interfaces;
using Winit.Modules.Location.BL.Interfaces;
using Winit.Modules.Location.Model.Classes;
using Winit.Modules.Location.Model.Interfaces;
using Winit.Shared.CommonUtilities.Extensions;
using Winit.Shared.Models.Common;
using Winit.Shared.Models.Enums;

namespace Winit.Modules.Location.BL.Classes;

public abstract class LocationBaseViewModel : ILocationViewModel
{
    // Injection
    private readonly IServiceProvider _serviceProvider;
    private readonly IAppUser _appUser;
    private readonly List<string> _propertiesToSearch = new();
    public List<ILocationItemView> LocationItemViews { get; set; }
    public List<ILocationType> LocationTypes { get; set; }
    public List<FilterCriteria> FilterCriterias { get; set; }

    protected LocationBaseViewModel(
        IServiceProvider serviceProvider,
            IFilterHelper filter,
            ISortHelper sorter,
            IListHelper listHelper,
            IAppUser appUser)
    {
        _serviceProvider = serviceProvider;
        _appUser = appUser;
        LocationItemViews = new List<ILocationItemView>();
        LocationTypes = new List<ILocationType>();
        FilterCriterias = new List<FilterCriteria>();
    }
    public async Task PopulateViewModel()
    {
        LocationTypes = await GetLocationTypes_Data();
        await PopulateGrid();
    }

    public async Task PopulateGrid()
    {
        LocationItemViews.Clear();
        List<ILocation> locations = await GetLocations_Data(null, 0);
        List<ILocationItemView> convertedlocationItemViews = ConvertToILocationItemView(locations);
        LocationItemViews.AddRange(convertedlocationItemViews);
    }
    public async Task<bool> CreateLocation(ILocationItemView locationItemView)
    {
        await GenerateLocationItemViewFields(locationItemView);
        return await CreateLocation_Data(locationItemView);
    }
    public async Task CreateLocationHierarchy(ILocationItemView locationItemView)
    {
        await CreateLocationHierarchyApiCall(locationItemView);
    }
    public async Task<bool> DeleteLocation(Winit.Modules.Location.Model.Interfaces.ILocationItemView locationItemView)
    {
        return await DeleteLocation_Data(locationItemView.UID);
    }
    public async Task<bool> UpdateLocation(Winit.Modules.Location.Model.Interfaces.ILocationItemView locationItemView)
    {
        AddUpdateFields(locationItemView);
        return await UpdateLocation_Data(locationItemView);
    }
    public List<ILocationItemView> ConvertToILocationItemView(List<ILocation> locations, string? ParentUID = null, string? ParentName = null, string? ParentLocationTypeUID = null)
    {
        List<ILocationItemView> locationItemViews = new();
        foreach (ILocation item in locations)
        {
            locationItemViews.Add(ConvertToILocationItemView(item, ParentUID, ParentName, ParentLocationTypeUID));
        }
        return locationItemViews;
    }
    public ILocationItemView ConvertToILocationItemView(ILocation location, string parentUID, string parentName, string? parentLocationTypeUID)
    {
        ILocationItemView locationItemView = new LocationItemView
        {
            Code = location.Code,
            Name = location.Name,
            ParentName = parentName,
            UID = location.UID,
            LocationTypeUID = location.LocationTypeUID,
            ParentLocationTypeUID = parentLocationTypeUID ?? "N/A",
            ParentUID = parentUID,
            SS = location.SS,
            CreatedBy = location.CreatedBy,
            CreatedTime = location.CreatedTime,
            ModifiedBy = location.ModifiedBy,
            ModifiedTime = location.ModifiedTime,
            ServerAddTime = location.ServerAddTime,
            ServerModifiedTime = location.ServerModifiedTime,
            ItemLevel = location.ItemLevel,
            HasChild = location.HasChild,
            LocationTypeCode = location.LocationTypeCode,
            LocationTypeName = location.LocationTypeName,
        };
        if (location.LocationTypeUID != null)
        {
            locationItemView.LocationTypeName = LocationTypes.Find(e => e.UID == location.LocationTypeUID)?.Name ?? string.Empty;
        }
        return locationItemView;
    }
    public async Task GenerateLocationItemViewFields(ILocationItemView locationItemView)
    {
        AddCreateFields(locationItemView, true);
        locationItemView.ChildGrids ??= new List<ILocationItemView>();
        await Task.CompletedTask;
    }
    public async Task GetChildGrid(Winit.Modules.Location.Model.Interfaces.ILocationItemView locationItemView)
    {
        if (locationItemView != null)
        {
            List<ILocation> locations = await GetLocations_Data(locationItemView.UID, locationItemView.ItemLevel + 1);
            locationItemView.ChildGrids = locations != null ? ConvertToILocationItemView(locations, locationItemView.UID, locationItemView.Name, locationItemView.LocationTypeUID) : new();
        }
    }
    public async Task<List<ISelectionItem>> GetLocationTypeSelectionItems(int Level, bool IsAddItembtn, string? ParentUID = null, bool IsAll = false)
    {
        if (LocationTypes != null)
        {
            if (IsAll)
            {
                return LocationTypes.Select(e => new SelectionItem
                {
                    UID = e.UID,
                    Label = e.Name,
                }).ToList<ISelectionItem>();
            }
            return IsAddItembtn
                ? LocationTypes.Where(i => i.ParentUID == null).Select(e => new SelectionItem
                {
                    UID = e.UID,
                    Label = e.Name,
                }).ToList<ISelectionItem>()
                : LocationTypes.Where(i => i.ParentUID == ParentUID && i.LevelNo == Level).Select(e => new SelectionItem
                {
                    UID = e.UID,
                    Label = e.Name,
                }).ToList<ISelectionItem>();
        }
        return await Task.FromResult(new List<ISelectionItem>());
    }
    public Task<ILocationItemView> CreateClone(Winit.Modules.Location.Model.Interfaces.ILocationItemView locationItemView)
    {
        ILocationItemView ClonedlocationItemView = _serviceProvider.CreateInstance<ILocationItemView>();
        ClonedlocationItemView.UID = locationItemView.UID;
        ClonedlocationItemView.SS = locationItemView.SS;
        ClonedlocationItemView.CreatedBy = locationItemView.CreatedBy;
        ClonedlocationItemView.ModifiedBy = locationItemView.ModifiedBy;
        ClonedlocationItemView.CreatedTime = locationItemView.CreatedTime;
        ClonedlocationItemView.ModifiedTime = locationItemView.ModifiedTime;
        ClonedlocationItemView.ServerAddTime = locationItemView.ServerAddTime;
        ClonedlocationItemView.ServerModifiedTime = locationItemView.ServerModifiedTime;
        ClonedlocationItemView.Code = locationItemView.Code;
        ClonedlocationItemView.Name = locationItemView.Name;
        ClonedlocationItemView.ParentUID = locationItemView.ParentUID;
        ClonedlocationItemView.ParentName = locationItemView.ParentName;
        ClonedlocationItemView.LocationTypeUID = locationItemView.LocationTypeUID;
        ClonedlocationItemView.LocationTypeName = locationItemView.LocationTypeName;
        ClonedlocationItemView.IsCreatePopUpOpen = locationItemView.IsCreatePopUpOpen;
        ClonedlocationItemView.IsDeletePopUpOpen = locationItemView.IsDeletePopUpOpen;
        ClonedlocationItemView.IsUpdatePopUpOpen = locationItemView.IsUpdatePopUpOpen;
        ClonedlocationItemView.IsOpen = locationItemView.IsOpen;
        ClonedlocationItemView.ParentLocationTypeUID = locationItemView.ParentLocationTypeUID;
        ClonedlocationItemView.ItemLevel = locationItemView.ItemLevel;
        return Task.Run(() => ClonedlocationItemView);
    }
    public async Task LocationTypeSelectedForParent(Winit.Modules.Location.Model.Interfaces.ILocationItemView Context, string UID)
    {
        await Task.Run(() => { Context.ItemLevel = LocationTypes.Find(e => e.UID == UID).LevelNo; });
    }
    public async Task ApplyFilter(IDictionary<string, string> keyValuePairs)
    {
        FilterCriterias.Clear();
        string? value;
        if (keyValuePairs.TryGetValue("locationhierarchy_level_code_name", out value) && !string.IsNullOrEmpty(value))
        {
            FilterCriterias.Add(new FilterCriteria("locationhierarchy_level_code_name", value, FilterType.Like, filterMode: FilterMode.Or));
        }
        await PopulateGrid();
    }
    #region Abstract Methods
    protected abstract Task<List<ILocationType>> GetLocationTypes_Data();
    protected abstract Task<List<ILocation>> GetLocations_Data(string? ParentUID, int level);
    protected abstract Task<bool> UpdateLocation_Data(ILocation location);
    protected abstract Task<bool> CreateLocation_Data(ILocation location);
    protected abstract Task CreateLocationHierarchyApiCall(ILocationItemView location);
    protected abstract Task<bool> DeleteLocation_Data(string locationUID);
    #endregion
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
}

