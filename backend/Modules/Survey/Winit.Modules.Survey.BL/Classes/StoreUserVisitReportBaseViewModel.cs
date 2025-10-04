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
using Winit.Shared.Models.Constants;
using Winit.Shared.CommonUtilities.Common;
using Winit.Modules.Location.Model.Interfaces;

namespace Winit.Modules.Survey.BL.Classes
{
    public abstract class StoreUserVisitReportBaseViewModel : IStoreUserVisitReportViewModel
    {
        public int PageNumber { get; set; } = 1;
        public int TotalItems { get; set; }
        public int PageSize { get; set; }
        public List<IStoreUserVisitDetails>? StoreUserVisitDetails { get; set; }
        public List<IStoreUserVisitDetails>? StoreUserVisitDetailsForExport { get; set; }
        protected FilterCriteria filterCriteria;
        public List<FilterCriteria> StoreUserVisitReportFilterCriterias { get; set; }
        public List<SortCriteria> SortCriterias { get; set; }
        public List<ISelectionItem> StateselectionItems { get; set; }
        public List<ISelectionItem> EmpSelectionList { get; set; }
        public List<ISelectionItem> StoreSelectionList { get; set; }
        public List<ISelectionItem> RoleSelectionList { get; set; }

        public string OrgUID { get; set; }

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
        public StoreUserVisitReportBaseViewModel(IServiceProvider serviceProvider,
            IFilterHelper filter,
            ISortHelper sorter,
            IListHelper listHelper, Winit.Shared.Models.Common.IAppConfig appConfigs, Winit.Modules.Base.BL.ApiService apiService,
            IAppUser appUser)
        {
            _serviceProvider = serviceProvider;
            _filter = filter;
            _sorter = sorter;
            _listHelper = listHelper;
            _apiService = apiService;
            _appConfigs = appConfigs;
            _appUser = appUser;
            StoreUserVisitDetails = new List<IStoreUserVisitDetails>();
            StateselectionItems = new List<ISelectionItem>();
            StoreUserVisitDetailsForExport = new List<IStoreUserVisitDetails>();
            StoreUserVisitReportFilterCriterias = new List<FilterCriteria>();
            SortCriterias = new List<SortCriteria>();
            EmpSelectionList = new List<ISelectionItem>();
            StoreSelectionList = new List<ISelectionItem>();
            RoleSelectionList = new List<ISelectionItem>();
        }

        public virtual async Task PopulateViewModel()
        {
            PagingRequest.PageNumber = 1;
            StoreUserVisitDetails.Clear();
            StoreUserVisitDetails.AddRange(await StoreUserVisitReportGridiview() ?? new());
        }
        public  async Task ExporttoExcel()
        {

            StoreUserVisitDetailsForExport.Clear();
            StoreUserVisitDetailsForExport.AddRange(await ExporttoreUserVisitReport() ?? new());
        }
        public async Task OnPageChange(int pageNumber)
        {
            PageNumber = pageNumber;
            await PopulateViewModel();
        }

        public async Task GetStores_Customers(string OrgUID)
        {
            StoreSelectionList.Clear();
            StoreSelectionList.AddRange(await GetStores_CustomersData(OrgUID));
        }
        public string GetDateOnlyInFormat(string value)
        {
            try
            {
                string dateValueString = value;
                DateTime dateValue;

                if (DateTime.TryParse(dateValueString, out dateValue))
                {
                    return dateValue.ToString("yyyy-MM-dd");
                    // Use the formattedDate as needed
                }
                return value;
            }
            catch (Exception ex)
            {
                throw;
            }
        }
        public async Task OnFilterApply(IDictionary<string, string> filterCriteria)
        {
            PageNumber = 1;
            List<FilterCriteria> filterCriterias = new List<FilterCriteria>();
            foreach (var keyValue in filterCriteria)
            {
                if (!string.IsNullOrEmpty(keyValue.Value))
                {
                    //if (keyValue.Key.Contains("VisitDateTime"))
                    //{
                    //    if (keyValue.Key == "VisitDateTime")
                    //        filterCriterias.Add(new FilterCriteria(keyValue.Key, GetDateOnlyInFormat(keyValue.Value), FilterType.Equal));
                    //}
                    if (keyValue.Key == "StartDate")
                    {
                        if (DateTime.TryParse(keyValue.Value, out DateTime parsedStartDate))
                        {
                            filterCriterias.Add(new FilterCriteria("VisitDate", parsedStartDate.ToString("yyyy-MM-dd"), FilterType.GreaterThanOrEqual));
                        }
                    }
                    else if (keyValue.Key == "EndDate")
                    {
                        if (DateTime.TryParse(keyValue.Value, out DateTime parsedEndDate))
                        {
                            filterCriterias.Add(new FilterCriteria("VisitDate", parsedEndDate.ToString("yyyy-MM-dd"), FilterType.LessThanOrEqual));
                        }
                    }
                    else if (keyValue.Key == "EmpCode")
                    {
                        ISelectionItem? selectionItem = EmpSelectionList
                            .Find(e => e.UID == keyValue.Value);

                        if (selectionItem != null)
                        {
                            filterCriterias.Add(new FilterCriteria(@$"{keyValue.Key}", selectionItem.Code, FilterType.Equal));
                        }
                    }
                    else
                        filterCriterias.Add(new FilterCriteria(@$"{keyValue.Key}", keyValue.Value, FilterType.Like));
                }
            }
             await ApplyFilter(filterCriterias);
        }
        public async Task ApplyFilter(List<FilterCriteria> filterCriterias)
        {
            StoreUserVisitReportFilterCriterias.Clear();
            StoreUserVisitReportFilterCriterias.AddRange(filterCriterias);
            await PopulateViewModel();
            await RefreshUserVisitStatusCountsAsync();
        }
        public async Task OnSort(SortCriteria sortCriteria)
        {
            PagingRequest.SortCriterias!.Clear();
            PagingRequest.SortCriterias.Add(sortCriteria);
            StoreUserVisitDetails.Clear();
            StoreUserVisitDetails.AddRange(await StoreUserVisitReportGridiview());
        }
        public int _plannedCount { get; set; }
        public int _visitedCount { get; set; }
        public int _unPlannedCount { get; set; }

        //public async Task RefreshUserVisitStatusCountsAsync()
        //{
        //    _plannedCount = (await GetUsers("Planned")).Count;
        //    _visitedCount = (await GetUsers("Visited")).Count;
        //    _unPlannedCount = (await GetUsers("null")).Count;

        //}
        public async Task RefreshUserVisitStatusCountsAsync()
        {
            var allUsers = await StoreTabUserVisitReportGridiview(null); // Fetch all users at once

            _plannedCount = allUsers.Count(u => u.Status == "Planned");
            _visitedCount = allUsers.Count(u => u.Status == "Visited");
            _unPlannedCount = allUsers.Count(u => string.IsNullOrEmpty(u.Status) || u.Status == "null");
        }

        public async Task<List<IStoreUserVisitDetails>> GetUsers(string status)
        {
            var result = await StoreTabUserVisitReportGridiview(status);

            return result ?? new List<IStoreUserVisitDetails>();
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
        public async Task GetSalesman(string OrgUID)
        {
            EmpSelectionList.Clear();
            EmpSelectionList.AddRange(await GetSalesmanData(OrgUID));
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

        public abstract Task<List<IStoreUserVisitDetails>> StoreUserVisitReportGridiview();
        public abstract Task<List<IStoreUserVisitDetails>> ExporttoreUserVisitReport();
        public abstract Task<List<IStoreUserVisitDetails>> StoreTabUserVisitReportGridiview(string status);
        public abstract Task<List<ILocation>> GetStateDetails(List<string> locationTypes);
        public abstract Task<List<ISelectionItem>> GetSalesmanData(string OrgUID);
        public abstract Task<List<ISelectionItem>> GetStores_CustomersData(string orgUID);


    }
}
