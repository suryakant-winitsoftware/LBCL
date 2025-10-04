using Winit.Modules.Base.BL.Helper.Interfaces;
using Winit.Modules.Common.BL.Interfaces;
using Winit.Modules.Common.Model.Interfaces;
using Winit.Modules.Location.BL.Interfaces;
using Winit.Modules.Location.Model.Classes;
using Winit.Modules.Location.Model.Interfaces;
using Winit.Modules.Setting.Model.Classes;
using Winit.Shared.CommonUtilities.Extensions;
using Winit.Shared.Models.Common;
using Winit.Shared.Models.Enums;
using Winit.Shared.Models.Events;

namespace Winit.Modules.Location.BL.Classes;

public abstract class CityBranchMappingBaseViewModel : ICityBranchMappingViewModel
{
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; }
    public int TotalItemsCount { get; set; }
    public List<Winit.Shared.Models.Common.ISelectionItem> BranchList { get; set; }
    public List<FilterCriteria> CityBranchFilterCriterias { get; set; }
    public List<SortCriteria> CityBranchSortCriterials { get; set; }
    public List<ICityBranchMapping> cityBranchMappinglist { get; set; }
    public List<ICityBranch> cityBrancheList { get; set; }
    private readonly IServiceProvider _serviceProvider;
    private readonly IFilterHelper _filter;
    private readonly ISortHelper _sorter;
    private readonly List<string> _propertiesToSearch = new();
    private readonly IListHelper _listHelper;
    private readonly IAppUser _appUser;
    private readonly IDataManager _dataManager;
    List<ISelectionItem> selectionItems = new();
    protected CityBranchMappingBaseViewModel(IServiceProvider serviceProvider,
           IFilterHelper filter,
           ISortHelper sorter,
           IListHelper listHelper,
           IAppUser appUser,
           IDataManager dataManager)
    {
        _serviceProvider = serviceProvider;
        _filter = filter;
        _sorter = sorter;
        _listHelper = listHelper;
        _appUser = appUser;
        _dataManager = dataManager;
        cityBrancheList = new List<ICityBranch>();
        BranchList = new List<ISelectionItem>();
        cityBranchMappinglist= new List<ICityBranchMapping>();
        CityBranchSortCriterials = new List<SortCriteria>();
        CityBranchFilterCriterias = new List<FilterCriteria>();
        //_propertiesToSearch.Add("StateCodeName");
        //_propertiesToSearch.Add("CityCodeName");
    }
    public async Task ApplyFilter(List<Shared.Models.Enums.FilterCriteria> filterCriterias)
    {
        CityBranchFilterCriterias.Clear();
        CityBranchFilterCriterias.AddRange(filterCriterias);
        await PopulateViewModel();
    }
    public async Task PopulateViewModel()
    {
        List<ICityBranch> data = await GetCityBranchDetails();
        if (data != null)
        {
            cityBrancheList.Clear();
            cityBrancheList.AddRange(data);
        }
    }
    public async Task SelectedBranchInDDL(DropDownEvent dropDownEvent)
    {
        
        if (dropDownEvent != null)
        {
            if (dropDownEvent.SelectionItems != null && dropDownEvent.SelectionItems.Count > 0)
            {
                selectionItems = dropDownEvent.SelectionItems;
            }
            else
            {
                selectionItems = new List<ISelectionItem>();
            }
        }
    }
    private List<ICityBranchMapping> MakingObjectForInsering()
    {
        ICityBranchMapping cityBranchMapping = _serviceProvider.CreateInstance<Model.Interfaces.ICityBranchMapping>();
        foreach (var selectionItem in selectionItems)
        {
            cityBranchMapping.UID = Guid.NewGuid().ToString();
            cityBranchMapping.CreatedBy = _appUser?.Emp?.UID;
            cityBranchMapping.ModifiedBy = _appUser?.Emp?.UID;
            cityBranchMapping.CreatedTime = DateTime.Now;
            cityBranchMapping.ModifiedTime = DateTime.Now;
            cityBranchMapping.ServerAddTime = DateTime.Now;
            cityBranchMapping.ServerModifiedTime = DateTime.Now;
            cityBranchMapping.SS = 0;
            cityBranchMapping.BranchLocationUID = selectionItem.UID;
            cityBranchMapping.CityLocationUID = selectionItem.UID;
            cityBranchMappinglist.Add(cityBranchMapping);
        }
        return cityBranchMappinglist;
    }
    public async Task PopulatetBranchDetails(string Uid)
    {
        BranchList = await GetBranchDetails(Uid);
    }
    public async Task  InsertCityBranchMapping()
    {
        MakingObjectForInsering();
        await CreateCityBranchMapping(cityBranchMappinglist);
    }
    public async Task ResetFilter()
    {
        CityBranchFilterCriterias.Clear();
        await PopulateViewModel();
    }
    public async Task ApplySort(List<Shared.Models.Enums.SortCriteria> sortCriterias)
    {
        await Task.CompletedTask;
    }
    public async Task PageIndexChanged(int pageNumber)
    {
        PageNumber = pageNumber;
        await PopulateViewModel();
    }
    #region Abstract Method
    protected abstract Task<List<ICityBranch>> GetCityBranchDetails();
    protected abstract Task<List<ISelectionItem>> GetBranchDetails(string UID);
    protected abstract  Task<bool> CreateCityBranchMapping(List<ICityBranchMapping> cityBranchMappings);
    #endregion
}

