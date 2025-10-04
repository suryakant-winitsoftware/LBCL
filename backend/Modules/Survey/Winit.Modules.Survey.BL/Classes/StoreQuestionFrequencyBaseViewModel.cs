using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Winit.Modules.Base.BL.Helper.Classes;
using Winit.Modules.Base.BL;
using Winit.Modules.Base.BL.Helper.Interfaces;
using Winit.Modules.Common.BL.Classes;
using Winit.Modules.Common.BL.Interfaces;
using Winit.Modules.Survey.BL.Interfaces;
using Winit.Shared.Models.Enums;
using Winit.Modules.Survey.Model.Interfaces;
using Winit.Modules.SKU.Model.Classes;
using Winit.Shared.Models.Common;
using Winit.Modules.Location.Model.Interfaces;
using Winit.Shared.CommonUtilities.Common;
using Winit.Shared.Models.Constants;

namespace Winit.Modules.Survey.BL.Classes
{
    public abstract class StoreQuestionFrequencyBaseViewModel : IStoreQuestionFrequencyViewModel
    {
        public int PageNumber { get; set; } = 1;
        public int TotalItems { get; set; }
        public int PageSize { get; set; }
        public List<IStoreQuestionFrequency> StoreQuestionFrequencyList { get; set; }
        protected FilterCriteria filterCriteria;
        public List<FilterCriteria> StoreQuestionFrequencyFilterCriterias { get; set; }
        public List<SortCriteria> SortCriterias { get; set; }
        public bool isFilterApplied { get; set; }
        public bool IsResetBtnClicked { get; set; }
        public bool IsPageChangeClicked { get; set; }
        public List<ISelectionItem> EmpSelectionList { get; set; }
        public List<ISelectionItem> StateselectionItems { get; set; } = new List<ISelectionItem>();
        public List<ISelectionItem> RoleSelectionList { get; set; }
        public bool IsExportClicked { get; set; }

        public List<ISelectionItem> Stores_CustSelectionList { get; set; }
        private IServiceProvider _serviceProvider;
        private readonly IFilterHelper _filter;
        private readonly ISortHelper _sorter;
        private readonly IListHelper _listHelper;
        private Winit.Shared.Models.Common.IAppConfig _appConfigs;
        private Winit.Modules.Base.BL.ApiService _apiService;
        IAppUser _appUser;
        public PagingRequest PagingRequest { get; set; } = new PagingRequest()
        {
            FilterCriterias = [],
            SortCriterias = [],
            IsCountRequired = true
        };
        public StoreQuestionFrequencyBaseViewModel(IServiceProvider serviceProvider,
            IFilterHelper filter,
            ISortHelper sorter,
            IListHelper listHelper, Winit.Shared.Models.Common.IAppConfig appConfigs, Winit.Modules.Base.BL.ApiService apiService,
            IAppUser appUser)
        {
            _serviceProvider = serviceProvider;
            _filter = filter; 
            _listHelper = listHelper;
            _apiService = apiService;
            _appConfigs = appConfigs;
            _appUser = appUser;
            StoreQuestionFrequencyList = new List<IStoreQuestionFrequency>();
            StoreQuestionFrequencyFilterCriterias = new List<FilterCriteria>();
            SortCriterias = new List<SortCriteria>();
            EmpSelectionList = new List<ISelectionItem>();
            Stores_CustSelectionList = new List<ISelectionItem>();
            StateselectionItems = new List<ISelectionItem>();
            RoleSelectionList = new List<ISelectionItem>();
        }

        public virtual async Task PopulateViewModel()
        {
            if (!isFilterApplied || IsResetBtnClicked|| IsPageChangeClicked)
            {
                var fromDate = DateTime.Today;
                var toDate = fromDate.AddDays(1);

                // Use "yyyy-MM-dd HH:mm:ss" format to match SQL
                var from = fromDate.ToString("yyyy-MM-dd HH:mm:ss");
                var to = toDate.ToString("yyyy-MM-dd HH:mm:ss");

                PagingRequest.FilterCriterias.Add(new FilterCriteria(
                    "ResponseDate",
                    new[] { from, to },
                    FilterType.Between
                ));
            }
            PagingRequest.PageNumber = 1;
            PagingRequest.PageSize = PageSize = 50;
            StoreQuestionFrequencyList.Clear();
            StoreQuestionFrequencyList.AddRange(await GetStoreQuestionFrequencyGridiview() ?? new());
        }
        public async Task OnPageChange(int pageNumber)
        {
            IsPageChangeClicked = true;
            PageNumber = pageNumber;
            await PopulateViewModel();
        }
        public async Task OnFilterApply(IDictionary<string, string> filterCriteria)
        {
            PageNumber = 1;
            List<FilterCriteria> filterCriterias = new List<FilterCriteria>();
            bool hasAnyFilter = false;

            foreach (var keyValue in filterCriteria)
            {
                if (!string.IsNullOrEmpty(keyValue.Value))
                {
                    isFilterApplied = true;
                    hasAnyFilter = true; // At least one filter is applied
                    if (keyValue.Key == "StartDate")
                    {
                        if (DateTime.TryParse(keyValue.Value, out DateTime parsedStartDate))
                        {
                            filterCriterias.Add(new FilterCriteria("ResponseDate", parsedStartDate.ToString("yyyy-MM-dd"), FilterType.GreaterThanOrEqual));
                        }
                    }
                    else if (keyValue.Key == "EndDate")
                    {
                        if (DateTime.TryParse(keyValue.Value, out DateTime parsedEndDate))
                        {
                            filterCriterias.Add(new FilterCriteria("ResponseDate", parsedEndDate.ToString("yyyy-MM-dd"), FilterType.LessThanOrEqual));
                        }
                    }
                    else
                    {
                        filterCriterias.Add(new FilterCriteria(@$"{keyValue.Key}", keyValue.Value, FilterType.Like));
                    }
                }
            }
            if (!hasAnyFilter)
            {
                IsResetBtnClicked = true;
                isFilterApplied = false;
            }
            await ApplyFilter(filterCriterias);
        }
        public async Task ApplyFilter(List<Shared.Models.Enums.FilterCriteria> filterCriterias)
        {
            StoreQuestionFrequencyFilterCriterias.Clear();
            StoreQuestionFrequencyFilterCriterias.AddRange(filterCriterias);
            await PopulateViewModel();
        }
        public async Task GetUsers(string OrgUID)
        {
            EmpSelectionList.Clear();
            EmpSelectionList.AddRange(await GetUsersData(OrgUID));
        }
        public async Task GetStores_Customers(string orgUID)
        {
            Stores_CustSelectionList.Clear();
            Stores_CustSelectionList.AddRange(await GetStores_CustomersData(orgUID));
        }
        private readonly Dictionary<string, string> StateCodeToNameMap = new()
        {
            { "Eastern", "Odisha(Eastern)" },
            { "EastIndia", "Bihar(EastIndia)" },
            { "NorthEastIndia", "Assam(NorthEastIndia)" },
            { "SouthBengal", "SouthBengal" },
            { "NorthBengal", "NorthBengal" },
            { "Region Sikkim", "Region Sikkim" },
            // Add all necessary mappings
        };
        public async Task GetStates()
        {
            try
            {
                StateselectionItems.Clear();
                // StateselectionItems = CommonFunctions.ConvertToSelectionItems(await GetStateDetails(new List<string> { "Region" }), new List<string> { "UID", "Code", "Name" });
                foreach (var item in CommonFunctions.ConvertToSelectionItems(await GetStateDetails(new List<string> { "Region" }), new List<string> { "Code", "Code", "Code" }))
                {
                    //StateselectionItems.Add(item);
                    if (StateCodeToNameMap.TryGetValue(item.Code, out var friendlyName))
                    {
                        item.Label = friendlyName;
                    }
                    StateselectionItems.Add(item);
                }
            }
            catch (Exception ex)
            {

            }
        }
        public async Task OnSort(SortCriteria sortCriteria)
        {
            PagingRequest.SortCriterias!.Clear();
            PagingRequest.SortCriterias.Add(sortCriteria);
            StoreQuestionFrequencyList.Clear();
            StoreQuestionFrequencyList.AddRange(await GetStoreQuestionFrequencyGridiview());
        }
        public async Task GetRoles()
        {
            RoleSelectionList.Clear();
            var rolesData = await GetRolesData();

            var fieldNames = new List<string> { "Code", "Code", "Code" };

            var selectionItems = CommonFunctions.ConvertToSelectionItems(rolesData, fieldNames);

            RoleSelectionList.AddRange(selectionItems);
        }

       
        public abstract Task<List<ISelectionItem>> GetRolesData();
        public abstract Task<List<ISelectionItem>> GetUsersData(string OrgUID);
        public abstract Task<List<ISelectionItem>> GetStores_CustomersData(string orgUID);
        public abstract Task<List<IStoreQuestionFrequency>> GetStoreQuestionFrequencyGridiview();
        public abstract Task<List<ILocation>> GetStateDetails(List<string> locationTypes);

        
    }
}
