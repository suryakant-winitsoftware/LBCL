using Winit.Modules.Base.BL.Helper.Interfaces;
using Winit.Modules.Common.BL.Interfaces;
using Winit.Modules.Location.BL.Interfaces;
using Winit.Modules.Location.Model.Classes;
using Winit.Modules.Location.Model.Interfaces;
using Winit.Shared.CommonUtilities.Extensions;
using Winit.Shared.Models.Enums;

namespace Winit.Modules.Location.BL.Classes;

public abstract class LocationTypeBaseViewModel : ILocationTypeViewModel
{
    // Injection
    private IServiceProvider _serviceProvider;
    private readonly IFilterHelper _filter;
    private readonly ISortHelper _sorter;
    private readonly IListHelper _listHelper;
    private readonly IAppUser _appUser;
    public List<ILocationTypeItemView> LocationTypeItemViews { get; set; }
    public List<FilterCriteria> FilterCriterias { get; set; }

    protected LocationTypeBaseViewModel(
            IServiceProvider serviceProvider,
            IFilterHelper filter,
            ISortHelper sorter,
            IListHelper listHelper,
            Winit.Modules.Common.BL.Interfaces.IAppUser appUser
    )
    {
        _serviceProvider = serviceProvider;
        _filter = filter;
        _sorter = sorter;
        _listHelper = listHelper;
        _appUser = appUser;
        LocationTypeItemViews = new List<ILocationTypeItemView>();
        FilterCriterias = new List<FilterCriteria>();
    }
    public async Task PopulateViewModel()
    {
        List<ILocationType> locationTypes = await GetLocationType_Data(null, 0);
        if (locationTypes != null && locationTypes.Any())
        {
            List<ILocationTypeItemView> convertedlocationTypeItemViews = ConvertToILocationTypeItemView(locationTypes);
            LocationTypeItemViews.AddRange(convertedlocationTypeItemViews);
        }
        foreach (var item in LocationTypeItemViews)
        {
            var secondLevel = await GetLocationType_Data(item.UID, item.LevelNo + 1);
            if (secondLevel != null)
            {
                item.ChildGrids = new List<ILocationTypeItemView>();
                item.ChildGrids.AddRange(ConvertToILocationTypeItemView(secondLevel, item.UID, item.Name));
            }
        }
    }
    public async Task<bool> CreateLocationType(ILocationTypeItemView locationTypeItemView)
    {
        await GenerateLocationTypeItemViewFields(locationTypeItemView);
        return await CreateLocationType_Data(locationTypeItemView);
    }
    public async Task<bool> DeleteLocationType(Winit.Modules.Location.Model.Interfaces.ILocationTypeItemView locationTypeItemView)
    {
        return await DeleteLocationType_Data(locationTypeItemView.UID);
    }
    public async Task<bool> UpdateLocationType(Winit.Modules.Location.Model.Interfaces.ILocationTypeItemView locationTypeItemView)
    {
        AddUpdateFields(locationTypeItemView);
        return await UpdateLocationType_Data(locationTypeItemView);
    }
    public List<ILocationTypeItemView> ConvertToILocationTypeItemView(List<ILocationType> locationTypes, string ParentUID = null, string ParentName = null, string? ParentLocationTypeTypeUID = null)
    {
        List<ILocationTypeItemView> locationTypeItemViews = new List<ILocationTypeItemView>();
        foreach (ILocationType item in locationTypes)
        {
            locationTypeItemViews.Add(ConvertToILocationTypeItemView(item, ParentUID, ParentName, ParentLocationTypeTypeUID));
        }
        return locationTypeItemViews;
    }
    public ILocationTypeItemView ConvertToILocationTypeItemView(ILocationType locationType, string ParentUID, string ParentName, string? ParentLocationTypeTypeUID)
    {
        ILocationTypeItemView locationTypeItemView = new LocationTypeItemView
        {
            Code = locationType.Code,
            LevelNo = locationType.LevelNo,
            Name = locationType.Name,
            ParentName = ParentName,
            UID = locationType.UID,
            ParentUID = ParentUID,
            SS = locationType.SS,
            CreatedBy = locationType.CreatedBy,
            CreatedTime = locationType.CreatedTime,
            ModifiedBy = locationType.ModifiedBy,
            ModifiedTime = locationType.ModifiedTime,
            ServerAddTime = locationType.ServerAddTime,
            ServerModifiedTime = locationType.ServerModifiedTime,
        };
        return locationTypeItemView;
    }
    public async Task GenerateLocationTypeItemViewFields(ILocationTypeItemView locationTypeItemView)
    {
        AddCreateFields(locationTypeItemView,true);
        if (locationTypeItemView.ChildGrids == null) locationTypeItemView.ChildGrids = new List<ILocationTypeItemView>();
        await Task.CompletedTask;
    }
    public async Task GetChildGrid(Winit.Modules.Location.Model.Interfaces.ILocationTypeItemView locationTypeItemView)
    {
        foreach (Winit.Modules.Location.Model.Interfaces.ILocationTypeItemView item in locationTypeItemView.ChildGrids)
        {
            List<ILocationType> locationTypes = await GetLocationType_Data(item.UID, item.LevelNo + 1);
            item.ChildGrids = locationTypes != null ? ConvertToILocationTypeItemView(locationTypes, item.UID, item.Name) : new();
        }
    }
    public Task<ILocationTypeItemView> CreateClone(Winit.Modules.Location.Model.Interfaces.ILocationTypeItemView locationTypeItemView)
    {
        ILocationTypeItemView ClonedlocationTypeItemView = _serviceProvider.CreateInstance<ILocationTypeItemView>();
        ClonedlocationTypeItemView.UID = locationTypeItemView.UID;
        ClonedlocationTypeItemView.SS = locationTypeItemView.SS;
        ClonedlocationTypeItemView.CreatedBy = locationTypeItemView.CreatedBy;
        ClonedlocationTypeItemView.ModifiedBy = locationTypeItemView.ModifiedBy;
        ClonedlocationTypeItemView.CreatedTime = locationTypeItemView.CreatedTime;
        ClonedlocationTypeItemView.ModifiedTime = locationTypeItemView.ModifiedTime;
        ClonedlocationTypeItemView.ServerAddTime = locationTypeItemView.ServerAddTime;
        ClonedlocationTypeItemView.ServerModifiedTime = locationTypeItemView.ServerModifiedTime;
        ClonedlocationTypeItemView.Code = locationTypeItemView.Code;
        ClonedlocationTypeItemView.Name = locationTypeItemView.Name;
        ClonedlocationTypeItemView.ParentUID = locationTypeItemView.ParentUID;
        ClonedlocationTypeItemView.ParentName = locationTypeItemView.ParentName;
        ClonedlocationTypeItemView.IsCreatePopUpOpen = locationTypeItemView.IsCreatePopUpOpen;
        ClonedlocationTypeItemView.IsDeletePopUpOpen = locationTypeItemView.IsDeletePopUpOpen;
        ClonedlocationTypeItemView.IsUpdatePopUpOpen = locationTypeItemView.IsUpdatePopUpOpen;
        ClonedlocationTypeItemView.IsOpen = locationTypeItemView.IsOpen;
        ClonedlocationTypeItemView.LevelNo = locationTypeItemView.LevelNo;
        return Task.Run(() => ClonedlocationTypeItemView);
    }
    public async Task ApplyFilter(IDictionary<string, string> keyValuePairs)
    {
        FilterCriterias.Clear();
        string? value;
        if (keyValuePairs.TryGetValue("locationhierarchy_type_code_name", out value) && !string.IsNullOrEmpty(value))
        {
            FilterCriterias.Add(new Shared.Models.Enums.FilterCriteria("locationhierarchy_type_code_name", value, 
                Shared.Models.Enums.FilterType.Like, filterMode: FilterMode.Or));
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
    protected abstract Task<List<ILocationType>> GetLocationType_Data(string? ParentUID, int Level);
    protected abstract Task<bool> UpdateLocationType_Data(ILocationType locationType);
    protected abstract Task<bool> DeleteLocationType_Data(string locationTypeUID);
    protected abstract Task<bool> CreateLocationType_Data(ILocationType locationType);
    #endregion
}

