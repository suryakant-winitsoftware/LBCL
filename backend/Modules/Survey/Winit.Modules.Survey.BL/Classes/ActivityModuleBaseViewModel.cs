using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Winit.Modules.Base.BL.Helper.Interfaces;
using Winit.Modules.Common.BL.Interfaces;
using Winit.Modules.Location.Model.Interfaces;
using Winit.Modules.Survey.BL.Interfaces;
using Winit.Modules.Survey.Model.Classes;
using Winit.Shared.CommonUtilities.Common;
using Winit.Shared.CommonUtilities.Extensions;
using Winit.Shared.Models.Common;
using Winit.Shared.Models.Enums;

namespace Winit.Modules.Survey.BL.Classes
{
    public abstract class ActivityModuleBaseViewModel:IActivityModuleViewModel
    {
        private IServiceProvider _serviceProvider;
        private readonly IFilterHelper _filter;
        private readonly ISortHelper _sorter;
        private readonly IListHelper _listHelper;
        private Winit.Shared.Models.Common.IAppConfig _appConfigs;
        private Winit.Modules.Base.BL.ApiService _apiService;
        IAppUser _appUser;
        public List<Winit.Modules.Survey.Model.Classes.ActivityModule> ActivityModuleList { get; set; }
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; }
        public int TotalItemsCount { get; set; }
        public List<FilterCriteria> ActivityModuleFilterCriterias { get; set; }
        public List<SortCriteria> SortCriterias { get; set; }
        public List<ISelectionItem> EmpSelectionList { get; set; }
        public List<ISelectionItem> Stores_CustSelectionList { get; set; }
        public List<ISelectionItem> StateselectionItems { get; set; } = new List<ISelectionItem>();
        public List<ISelectionItem> RoleSelectionList { get; set; }
        public bool IsExportClicked { get; set; }

        public ActivityModuleBaseViewModel(IServiceProvider serviceProvider,
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
            ActivityModuleList = new List<ActivityModule>();
            ActivityModuleFilterCriterias = new List<FilterCriteria>();
            SortCriterias = new List<SortCriteria>();
            EmpSelectionList = new List<ISelectionItem>();
            Stores_CustSelectionList = new List<ISelectionItem>();
            StateselectionItems = new List<ISelectionItem>();
            RoleSelectionList = new List<ISelectionItem>();
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
        public async Task ApplySort(SortCriteria sortCriteria)
        {
            SortCriterias.Clear();
            SortCriterias.Add(sortCriteria);
            await GetActivityModuleData();
        }
        public async Task OnFilterApply(Dictionary<string, string> keyValuePairs)
        {
            try
            {
                ActivityModuleFilterCriterias.Clear();
                foreach (var keyValue in keyValuePairs)
                {
                    if (!string.IsNullOrEmpty(keyValue.Value))
                    {
                        if (keyValue.Key == "StartDate")
                        {
                            if (DateTime.TryParse(keyValue.Value, out DateTime parsedStartDate))
                            {
                                ActivityModuleFilterCriterias.Add(new FilterCriteria("Date", parsedStartDate.ToString("yyyy-MM-dd"), FilterType.GreaterThanOrEqual));
                            }
                        }
                        else if (keyValue.Key == "EndDate")
                        {
                            if (DateTime.TryParse(keyValue.Value, out DateTime parsedEndDate))
                            {
                                ActivityModuleFilterCriterias.Add(new FilterCriteria("Date", parsedEndDate.ToString("yyyy-MM-dd"), FilterType.LessThanOrEqual));
                            }
                        }

                        else
                        {
                            ActivityModuleFilterCriterias.Add(new FilterCriteria(keyValue.Key, keyValue.Value, FilterType.Like));
                        }
                    }
                }
                PageNumber = 1;
                await GetActivityModuleData();
            }
            catch (Exception ex)
            {

            }
        }
        public virtual async Task GetActivityModuleData()
        {
            ActivityModuleList.Clear();
            ActivityModuleList.AddRange(await GetActivityModuleDataDetails() ?? new());
        }
        public async Task PageIndexChanged(int pageNumber)
        {
            PageNumber = pageNumber;
            await GetActivityModuleData();
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
        public async Task GetRoles()
        {
            RoleSelectionList.Clear();
            var rolesData = await GetRolesData();

            var fieldNames = new List<string> { "Code", "Code", "Code" };

            var selectionItems = CommonFunctions.ConvertToSelectionItems(rolesData, fieldNames);

            RoleSelectionList.AddRange(selectionItems);
        }
        public abstract Task<List<ILocation>> GetStateDetails(List<string> locationTypes);

        public abstract Task<List<ISelectionItem>> GetUsersData(string OrgUID);
        public abstract Task<List<ISelectionItem>> GetStores_CustomersData(string orgUID);
        public abstract Task<List<Model.Classes.ActivityModule>> GetActivityModuleDataDetails();
        public abstract Task<List<ISelectionItem>> GetRolesData();

    }
}
