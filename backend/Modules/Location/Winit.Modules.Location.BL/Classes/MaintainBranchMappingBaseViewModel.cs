using System.Collections.Generic;
using Winit.Modules.Base.BL.Helper.Interfaces;
using Winit.Modules.Common.BL.Interfaces;
using Winit.Modules.Location.BL.Interfaces;
using Winit.Modules.Location.Model.Interfaces;
using Winit.Shared.Models.Common;
using Winit.Shared.Models.Enums;

namespace Winit.Modules.Location.BL.Classes
{
    public abstract class MaintainBranchMappingBaseViewModel : IMaintainBranchMappingViewModel
    {
        private IServiceProvider _serviceProvider;
        private readonly IFilterHelper _filter;
        private readonly ISortHelper _sorter;
        private readonly IListHelper _listHelper;
        private Winit.Shared.Models.Common.IAppConfig _appConfigs;
        private Winit.Modules.Base.BL.ApiService _apiService;
        private List<string> _propertiesToSearch = new List<string>();
        protected List<Winit.Modules.Location.Model.Classes.Location> LocationsByType { get; set; }
        private Winit.Modules.Common.BL.Interfaces.IAppUser _appUser;
        public int TotalItems { get; set; }
        public List<IBranch> BranchDetailsList { get; set; }
        public List<ILocation> StatesForSelection { get; set; }
        public List<ILocation> CitiesForSelection { get; set; }
        public List<ILocation> LocalitiesForSelection { get; set; }
        public List<ISalesOffice> SalesOfficeDetails { get; set; }
        public List<ISelectionItem> SalesOfficeOrgType { get; set; }
        public List<ISalesOffice> CompleteSalesOfficeDetailsList { get; set; }

        public IBranch ViewBranchDetails { get; set; }
        public ISalesOffice SalesOffice { get; set; }
        public PagingRequest PagingRequest { get; set; } = new PagingRequest()
        {
            FilterCriterias = [],
            SortCriterias = [],
            IsCountRequired = true
        };

        public MaintainBranchMappingBaseViewModel(IServiceProvider serviceProvider,
           IFilterHelper filter,
        ISortHelper sorter,
            IAppUser appUser,
           IListHelper listHelper, Winit.Shared.Models.Common.IAppConfig appConfigs, Winit.Modules.Base.BL.ApiService apiService
         )
        {
            _serviceProvider = serviceProvider;
            _filter = filter;
            _sorter = sorter;
            _listHelper = listHelper;
            _apiService = apiService;
            _appConfigs = appConfigs;
            _appUser = appUser;
            BranchDetailsList = new List<IBranch>();
            StatesForSelection = new List<ILocation>();
            CitiesForSelection = new List<ILocation>();
            LocalitiesForSelection = new List<ILocation>();
            SalesOfficeDetails = new List<ISalesOffice>();
            SalesOfficeOrgType = new List<ISelectionItem>();
            CompleteSalesOfficeDetailsList = new List<ISalesOffice>();
            // Property set for Search
            _propertiesToSearch.Add("Code");
            _propertiesToSearch.Add("Name");
        }
        public async virtual Task PopulateViewModel()
        {
            BranchDetailsList.Clear();
            BranchDetailsList.AddRange(await GetBranchDetailsData());

        }
        public async virtual Task GetStatesForSelection()
        {
            await GetStatesData();
        }
        public async virtual Task GetCitiesForSelection(List<ILocation> selectedStates)
        {
            await GetCitiesDataViaSelectedStates(selectedStates);
        }
        public async virtual Task GetLocalitiesForSelection(List<ILocation> selectedCities)
        {
            await GetLocalitiesDataViaSelectedStates(selectedCities);
        }
        public async virtual Task PopulateBranchDetailsByUID(string UID)
        {
            ViewBranchDetails = await GetBranchDatailsDataByUID(UID);

        }

        public async Task OnFilterApply(IDictionary<string, string> filterCriteria)
        {
            if (filterCriteria != null)
            {
                PagingRequest.FilterCriterias!.Clear();
                foreach (var keyValue in filterCriteria)
                {
                    if (!string.IsNullOrEmpty(keyValue.Value))
                    {
                        if (keyValue.Value.Contains(","))
                        {
                            string[] values = keyValue.Value.Split(',');
                            PagingRequest.FilterCriterias.Add(new FilterCriteria(keyValue.Key, values, FilterType.In));
                        }
                        else
                        {
                            PagingRequest.FilterCriterias.Add(new FilterCriteria(keyValue.Key, keyValue.Value, FilterType.Like));
                        }
                    }
                }
                BranchDetailsList.Clear();
                BranchDetailsList.AddRange(await GetBranchDetailsData());
            }

        }
        public async Task OnSort(SortCriteria sortCriteria)
        {
            PagingRequest.SortCriterias!.Clear();
            PagingRequest.SortCriterias.Add(sortCriteria);
            BranchDetailsList.Clear();
            BranchDetailsList.AddRange(await GetBranchDetailsData());
        }
        public async Task OnPageChange(int pageNumber)
        {
            PagingRequest.PageNumber = pageNumber;
            BranchDetailsList.Clear();
            BranchDetailsList.AddRange(await GetBranchDetailsData());
        }
        public async virtual Task GetBranchMappingSalesOffices(string UID)
        {
            CompleteSalesOfficeDetailsList = await GetCompleteSalesOfficeDetailsList();
            SalesOfficeDetails = await GetSalesOfficeDataByBranchUID(UID);
            SalesOfficeOrgType = await GetSalesOfficeOrgTypesForSelection("WH");
        }
        public abstract Task<List<ISalesOffice>> GetCompleteSalesOfficeDetailsList();
        public abstract Task<List<Winit.Modules.Location.Model.Interfaces.IBranch>> GetBranchDetailsData();
        public abstract Task<Winit.Modules.Location.Model.Interfaces.IBranch> GetBranchDatailsDataByUID(string UID);
        protected abstract Task GetStatesData();
        protected abstract Task GetCitiesDataViaSelectedStates(List<ILocation> selectedStates);
        protected abstract Task GetLocalitiesDataViaSelectedStates(List<ILocation> selectedCities);
        public abstract Task<bool> SaveOrUpdateBranchDetails(IBranch viewBranchDetails, bool @operator);
        public abstract Task<bool> SaveStoreDetailsDetails(ISalesOffice salesOffice);
        public abstract Task<List<ISalesOffice>> GetSalesOfficeDataByBranchUID(string UID);
        public abstract Task<List<ISelectionItem>> GetSalesOfficeOrgTypesForSelection(string orgTypeUID);
        public abstract Task<bool> DeleteSalesOfficeDetails(ISalesOffice salesOffice);

    }
}
