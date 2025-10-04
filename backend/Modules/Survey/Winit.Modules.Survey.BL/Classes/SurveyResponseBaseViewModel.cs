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
using Nest;
using RabbitMQ.Client;
using Winit.Shared.CommonUtilities.Common;
using Winit.Modules.Location.Model.Interfaces;
using Winit.Modules.JourneyPlan.Model.Interfaces;

namespace Winit.Modules.Survey.BL.Classes
{
    public abstract class SurveyResponseBaseViewModel : ISurveyResponseViewModel
    {
        public string Role { get; set; }
        public string PageType { get; set; }
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; }
        public int TotalItems { get; set; }
        public int TotalTabItems { get; set; }

        public List<FilterCriteria> ViewSurveyFilterCriterias { get; set; }
        public List<IViewSurveyResponse> SurveyResponsesList { get; set; }
        public List<IViewSurveyResponse> TabCountforTotalCategoryList { get; set; }
        public List<IViewSurveyResponseExport> ExporttoExcelSurveyResponsesList { get; set; }
        public ISurveyResponseViewDTO ViewSurveyResponsesDTOList { get; set; }
        public string OrgUID { get; set; }
        public List<ISelectionItem> EmpSelectionList { get; set; }
        public List<ISelectionItem> Stores_CustSelectionList { get; set; }
        public List<ISelectionItem> StateselectionItems { get; set; } = new List<ISelectionItem>();
        public List<ISelectionItem> RoleSelectionList { get; set; }
        public List<ISelectionItem> QuestionsList { get; set; }


        protected FilterCriteria filterCriteria;

        public PagingRequest PagingRequest { get; set; } = new PagingRequest()
        {
            FilterCriterias = [],
            SortCriterias = [],
            IsCountRequired = true,
           
        };
        public List<SortCriteria> SortCriterias { get; set; }
        protected FilterCriteria DefaultCriteria { get; set; }
        protected SortCriteria DefaultSortCriteria { get; set; }

        private IServiceProvider _serviceProvider;
        private readonly IFilterHelper _filter;
        private readonly ISortHelper _sorter;
        private readonly IListHelper _listHelper;
        private Winit.Shared.Models.Common.IAppConfig _appConfigs;
        private Winit.Modules.Base.BL.ApiService _apiService;
        IAppUser _appUser;
        public SurveyResponseBaseViewModel(IServiceProvider serviceProvider,
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
            SurveyResponsesList = new List<IViewSurveyResponse>();
            EmpSelectionList = new List<ISelectionItem>();
            StateselectionItems = new List<ISelectionItem>();
            Stores_CustSelectionList = new List<ISelectionItem>();
            RoleSelectionList = new List<ISelectionItem>();
            QuestionsList = new List<ISelectionItem>();
            _filteredSurveyResponseList = new List<IViewSurveyResponse>();
            TabCountforTotalCategoryList = new List<IViewSurveyResponse>();
            ExporttoExcelSurveyResponsesList = new List<IViewSurveyResponseExport>();
            ViewSurveyResponsesDTOList = new Winit.Modules.Survey.Model.Classes.SurveyResponseViewDTO();
            SortCriterias = new List<SortCriteria>();
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
        public async Task GetRoles()
        {
            RoleSelectionList.Clear();
            var rolesData = await GetRolesData();

            var fieldNames = new List<string> { "Code", "Code", "Code" };

            var selectionItems = CommonFunctions.ConvertToSelectionItems(rolesData, fieldNames);

            RoleSelectionList.AddRange(selectionItems);
        }
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
        public virtual async Task PopulateViewModel()
        {
            PagingRequest.PageNumber  = 1;
            PagingRequest.PageSize = PageSize = 50;
            DefaultSortCriteria = new SortCriteria(nameof(IViewSurveyResponse.CreatedDateTime), SortDirection.Desc);
            PagingRequest.SortCriterias.Add(DefaultSortCriteria);
            DefaultCriteria = new FilterCriteria(nameof(IViewSurveyResponse.ActivityType), PageType, FilterType.Equal);
            PagingRequest.FilterCriterias.Add(DefaultCriteria);
            if (!IsPageIndexapplied )
            {
                await GetData(applyTodayFilter: true);
            }
            if (PageType == "RaiseTicket")
            {
               // GetSurveyAge();
            }
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
        public virtual async Task ApplyPageIndexChanged()
        {
            PagingRequest.PageNumber = 1;
            PagingRequest.PageSize = PageSize = 50;
            DefaultSortCriteria = new SortCriteria(nameof(IViewSurveyResponse.CreatedDateTime), SortDirection.Desc);
            PagingRequest.SortCriterias.Add(DefaultSortCriteria);
            DefaultCriteria = new FilterCriteria(nameof(IViewSurveyResponse.ActivityType), PageType, FilterType.Equal);
            PagingRequest.FilterCriterias.Add(DefaultCriteria);
            await GetSurveyResponseDetails();
            //if (PageType == "RaiseTicket")
            //{
            //    GetSurveyAge();
            //}
        }
        private void GetSurveyAge()
        {
            if (SurveyResponsesList != null && SurveyResponsesList.Any())
            {
                foreach (var item in SurveyResponsesList)
                {
                    item.SurveyAge = CalculateSurveyAge(item.CreatedDateTime);
                }
            }
        }
        private string CalculateSurveyAge(DateTime createdTime)
        {
            var span = DateTime.Now - createdTime;

            if (span.TotalMinutes < 1)
                return "just now";
            else if (span.TotalMinutes < 60)
                return $"{(int)span.TotalMinutes} min(s) ago";
            else if (span.TotalHours < 24)
                return $"{(int)span.TotalHours} hour(s) ago";
            else
                return $"{(int)span.TotalDays} day(s) ago";
        }
        private async Task GetData(bool applyTodayFilter = false)
        {
            if (!PagingRequest.FilterCriterias.Any(fc => fc.Name == "CreatedDateTime"))

            {
                var fromDate = DateTime.Today;
                var toDate = fromDate.AddDays(1);

                // Use "yyyy-MM-dd HH:mm:ss" format to match SQL
                var from = fromDate.ToString("yyyy-MM-dd HH:mm:ss");
                var to = toDate.ToString("yyyy-MM-dd HH:mm:ss");

                PagingRequest.FilterCriterias.Add(new FilterCriteria(
                    "CreatedDateTime",
                    new[] { from, to },
                    FilterType.Between
                ));
                await Task.WhenAll(
                         //GetSurveyResponseDetailsForExport(),
                         GetSurveyResponseDetails()
                 );
            }
            else
            {
                await Task.WhenAll(
                        //GetSurveyResponseDetailsForExport(),
                        GetSurveyResponseDetails()
                        );
            }
        }
            
        public bool isFilterApplied { get; set; }
        public async Task GetSurveyResponseDetailsForExport()
        {
            if (!isFilterApplied)
            {
                var fromDate = DateTime.Today;
                var toDate = fromDate.AddDays(1);

                // Use "yyyy-MM-dd HH:mm:ss" format to match SQL
                var from = fromDate.ToString("yyyy-MM-dd HH:mm:ss");
                var to = toDate.ToString("yyyy-MM-dd HH:mm:ss");

                PagingRequest.FilterCriterias.Add(new FilterCriteria(
                    "CreatedDateTime",
                    new[] { from, to },
                    FilterType.Between
                ));
            }
            DefaultSortCriteria = new SortCriteria(nameof(IViewSurveyResponse.CreatedDateTime), SortDirection.Desc);
            PagingRequest.SortCriterias.Add(DefaultSortCriteria);
            DefaultCriteria = new FilterCriteria(nameof(IViewSurveyResponse.ActivityType), PageType, FilterType.Equal);
            PagingRequest.FilterCriterias.Add(DefaultCriteria);
            ExporttoExcelSurveyResponsesList.Clear();
            ExporttoExcelSurveyResponsesList.AddRange(await GetSurveyResponseDetailsExportToExcel() ?? new());
        }
        private async Task GetSurveyResponseDetails()
        {
            SurveyResponsesList.Clear();
            SurveyResponsesList.AddRange(await GetSurveyResponseDetailsGridiview() ?? new());
        }
        public bool IspageInitalLoad { get; set; }
        public bool IsFilerappliedForTabData { get; set; }
        public bool IsPageIndexapplied { get; set; }

        public async Task GetTabDataAfterFilterApply()
        {
            //PagingRequest.FilterCriterias.Clear(); // Clear previous filters
            PagingRequest.PageSize = int.MaxValue;
            PagingRequest.PageNumber = 1;
            // Filter by ActivityType
            var defaultCriteria = new FilterCriteria(nameof(IViewSurveyResponse.ActivityType), PageType, FilterType.Equal);
            PagingRequest.FilterCriterias.Add(defaultCriteria);
            if(IspageInitalLoad)
            {
                var fromDate = DateTime.Today;
                var toDate = fromDate.AddDays(1);

                // Use "yyyy-MM-dd HH:mm:ss" format to match SQL
                var from = fromDate.ToString("yyyy-MM-dd HH:mm:ss");
                var to = toDate.ToString("yyyy-MM-dd HH:mm:ss");

                PagingRequest.FilterCriterias.Add(new FilterCriteria(
                    "CreatedDateTime",
                    new[] { from, to },
                    FilterType.Between
                ));
            }
            _filteredSurveyResponseList.Clear();
            _filteredSurveyResponseList.AddRange(await GetTabDataAfterFilterApplyFromAPI() ?? new());
            var customerCount = _filteredSurveyResponseList
             .Where(r => !string.IsNullOrWhiteSpace(r.CustomerCode) && !string.IsNullOrWhiteSpace(r.UserCode))
             .Select(r => new { r.CustomerCode, r.UserCode })
             .Distinct()
             .Count() * 5;

            _TotalCategoryCount = customerCount;
            _ExcecutedCount = _filteredSurveyResponseList.Count;
           // _pendingCount = _TotalCategoryCount - _ExcecutedCount;
        }
        public async Task RefreshUserVisitStatusCountsAsync()
        {
            PagingRequest.FilterCriterias.Clear(); // Clear previous filters
            PagingRequest.PageSize = int.MaxValue;
            PagingRequest.PageNumber = 1;
            // Filter by ActivityType
            var defaultCriteria = new FilterCriteria(nameof(IViewSurveyResponse.ActivityType), PageType, FilterType.Equal);
            PagingRequest.FilterCriterias.Add(defaultCriteria);

            // Set date range (today and tomorrow) in correct format
            var fromDate = DateTime.Today;
            var toDate = fromDate.AddDays(1);

            // Use "yyyy-MM-dd HH:mm:ss" format to match SQL
            var from = fromDate.ToString("yyyy-MM-dd HH:mm:ss");
            var to = toDate.ToString("yyyy-MM-dd HH:mm:ss");

            PagingRequest.FilterCriterias.Add(new FilterCriteria(
                "CreatedDateTime",
                new[] { from, to },
                FilterType.Between
            ));

            // _filteredSurveyResponseList = await GetUsers(); // Fetch filtered data
            _filteredSurveyResponseList = await GetTabDataForInitialpageLoadFromAPI(); // Fetch filtered data
          
        }
        public async Task OnPageChange(int pageNumber)
        {
            PagingRequest.PageNumber= PageNumber  = pageNumber;
            IsPageIndexapplied = true;
            PagingRequest.PageSize = PageSize ;
            DefaultSortCriteria = new SortCriteria(nameof(IViewSurveyResponse.CreatedDateTime), SortDirection.Desc);
            PagingRequest.SortCriterias.Add(DefaultSortCriteria);
            DefaultCriteria = new FilterCriteria(nameof(IViewSurveyResponse.ActivityType), PageType, FilterType.Equal);
            PagingRequest.FilterCriterias.Add(DefaultCriteria); 
            await GetSurveyResponseDetails();

        }
        public virtual async Task ViewSurveyResponsePopulateViewModel(string SurveyUID)
        {
            ViewSurveyResponsesDTOList = await GetViewSurveyResponseDetailsGridiview(SurveyUID);
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
        public List<IViewSurveyResponse> _filteredSurveyResponseList { get; set; }

        public int _TotalCategoryCount { get; set; }
        public int _ExcecutedCount { get; set; }
        public int _pendingCount { get; set; }
        public bool noFilters { get; set; }
        public List<Winit.Modules.Survey.Model.Classes.ActivityModule> ActivityModuleList { get; set; }

        public async Task OnFilterApply(IDictionary<string, string> filterCriteria)
        {
            IspageInitalLoad = false;
            isFilterApplied = true;
            PagingRequest.FilterCriterias.Clear();
            PagingRequest.PageNumber = PageNumber = 1;
             noFilters = filterCriteria == null || filterCriteria.All(kv => string.IsNullOrWhiteSpace(kv.Value));

            if (noFilters)
            {
                await PopulateViewModel();
                await GetTabDataAfterFilterApply();
                return;
            }
            foreach (var keyValue in filterCriteria)
            {
                if (!string.IsNullOrEmpty(keyValue.Value))
                {
                    // Handle date range filtering
                    if (keyValue.Key == "StartDate")
                    {
                        if (DateTime.TryParse(keyValue.Value, out DateTime parsedStartDate))
                        {
                            PagingRequest.FilterCriterias.Add(new FilterCriteria("CreatedDate", parsedStartDate.ToString("yyyy-MM-dd"), FilterType.GreaterThanOrEqual));
                        }
                    }
                    else if (keyValue.Key == "EndDate")
                    {
                        if (DateTime.TryParse(keyValue.Value, out DateTime parsedEndDate))
                        {
                            PagingRequest.FilterCriterias.Add(new FilterCriteria("CreatedDate", parsedEndDate.ToString("yyyy-MM-dd"), FilterType.LessThanOrEqual));
                        }
                    }
                    else if (keyValue.Key == "UserCode")
                    {
                        ISelectionItem? selectionItem = EmpSelectionList
                            .Find(e => e.UID == keyValue.Value);

                        if (selectionItem != null)
                        {
                            PagingRequest.FilterCriterias.Add(new FilterCriteria(@$"{keyValue.Key}", selectionItem.Code, FilterType.Equal));
                        }
                    }
                    
                    else
                    {
                        if (keyValue.Key == "Status")
                        {
                            //if (keyValue.Value == "Open")
                            //{
                            //    // If the value is "Open", treat it as IS NULL
                            //    PagingRequest.FilterCriterias.Add(new FilterCriteria(keyValue.Key, "", FilterType.Equal));
                            //}
                            //else
                            //{
                                PagingRequest.FilterCriterias.Add(new FilterCriteria(keyValue.Key, keyValue.Value, FilterType.Equal));
                            //}
                        }
                        else
                        {
                            // Default to LIKE filter
                            PagingRequest.FilterCriterias.Add(new FilterCriteria(keyValue.Key, keyValue.Value, FilterType.Like));
                        }
                    }
                }
            }
            PagingRequest.FilterCriterias.Add(DefaultCriteria);
            //await GetData();
            await GetSurveyResponseDetails();
            if (IsFilerappliedForTabData)
            {
                await GetTabDataAfterFilterApply();
            }
           // GetSurveyAge();
        }
     
        public async Task ApplySort(SortCriteria sortCriteria)
        {
            SortCriterias.Clear();
            SortCriterias.Add(sortCriteria);
            if (!isFilterApplied)
            {
                var fromDate = DateTime.Today;
                var toDate = fromDate.AddDays(1);

                // Use "yyyy-MM-dd HH:mm:ss" format to match SQL
                var from = fromDate.ToString("yyyy-MM-dd HH:mm:ss");
                var to = toDate.ToString("yyyy-MM-dd HH:mm:ss");

                PagingRequest.FilterCriterias.Add(new FilterCriteria(
                    "CreatedDateTime",
                    new[] { from, to },
                    FilterType.Between
                ));
            }
            PagingRequest.SortCriterias=SortCriterias;
            //GetSurveyAge();
            await GetSurveyResponseDetails();
        }
        public async Task<List<ISelectionItem>> GetQuestionLabelsBySurveyUid(string selectedSurveyUid)
        {
            QuestionsList.Clear();
            var questions = await GetQuestionsData(selectedSurveyUid);
            QuestionsList.AddRange(questions ?? new List<ISelectionItem>());
            return QuestionsList;
        }


        public abstract Task<List<IViewSurveyResponse>> GetSurveyResponseDetailsGridiview();
        public abstract Task<List<IViewSurveyResponse>> GetTabDataAfterFilterApplyFromAPI();
        public abstract Task<List<IViewSurveyResponseExport>> GetSurveyResponseDetailsExportToExcel();
        public abstract Task<ISurveyResponseViewDTO> GetViewSurveyResponseDetailsGridiview(string SurveyUID);
        public abstract Task<List<IViewSurveyResponse>> GetTabDataForInitialpageLoadFromAPI();
        public abstract Task<List<ISelectionItem>> GetUsersData(string OrgUID);
        public abstract Task<List<ISelectionItem>> GetStores_CustomersData(string orgUID);
        public abstract Task<List<ILocation>> GetStateDetails(List<string> locationTypes);
        public abstract Task<List<ISelectionItem>> GetRolesData();
        public abstract Task<List<ISelectionItem>> GetQuestionsData(string selectedSurveyUid);

    }
}
