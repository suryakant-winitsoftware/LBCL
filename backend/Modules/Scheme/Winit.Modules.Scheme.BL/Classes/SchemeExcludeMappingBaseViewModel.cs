using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Winit.Modules.Base.BL.Helper.Interfaces;
using Winit.Modules.Base.BL;
using Winit.Modules.Common.BL.Interfaces;
using Winit.Modules.Common.Model.Interfaces;
using Winit.Modules.Scheme.BL.Interfaces;
using Winit.Modules.Scheme.Model.Interfaces;
using Winit.Modules.Setting.BL.Interfaces;
using Winit.Modules.SKU.BL.Interfaces;
using Winit.UIComponents.Common;
using Winit.UIComponents.SnackBar;
using Winit.Shared.Models.Common;
using Winit.UIComponents.Common.Services;
using Winit.Shared.CommonUtilities.Common;
using Winit.Modules.Scheme.Model.Classes;
using Winit.UIComponents.SnackBar.Services;
using Winit.Shared.Models.Enums;

namespace Winit.Modules.Scheme.BL.Classes
{
    public abstract class SchemeExcludeMappingBaseViewModel : ISchemeExcludeMappingViewModel
    {
        protected readonly Winit.Shared.Models.Common.IAppConfig _appConfig;
        protected readonly ApiService _apiService;
        protected readonly IServiceProvider _serviceProvider;
        protected readonly IAppUser _appUser;
        protected readonly ILoadingService _loadingService;
        protected readonly IAlertService _alertService;
        protected readonly IAppSetting _appSetting;
        protected readonly CommonFunctions _commonFunctions;
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 10;
        public int TotalItemsCount { get; set; }
        public List<ISchemeExcludeMapping> SchemeExcludeGridViewRecords { get; set; }
        public List<SchemeExcludeMapping> SchemeExcludeErrorRecords { get; set; }
        public List<FilterCriteria> FilterCriterias { get; set; }
        public List<SortCriteria> SortCriterias { get; set; }
        public SortCriteria DefaultSortCriteria = new("CreatedTime", SortDirection.Asc);

        public SchemeExcludeMappingBaseViewModel(IServiceProvider serviceProvider, IFilterHelper filter, ISortHelper sorter, IListHelper listHelper, IAppUser appUser, IAppSetting appSetting,
        IDataManager dataManager, Shared.Models.Common.IAppConfig appConfigs, ApiService apiService, CommonFunctions commonFunctions)
        {
            _apiService = apiService;
            _appConfig = appConfigs;
            _serviceProvider = serviceProvider;
            _appUser = appUser;
            _appSetting = appSetting;
            _commonFunctions = commonFunctions;
            SchemeExcludeGridViewRecords = new List<ISchemeExcludeMapping>();
            SchemeExcludeErrorRecords = new List<SchemeExcludeMapping>();
            SortCriterias = new List<SortCriteria>();
            FilterCriterias = new List<FilterCriteria>();
        }
        public async Task PopulateViewModel()
        {
            try
            {
                SchemeExcludeGridViewRecords = await PopulateUI();
            }
            catch (Exception)
            {
                throw;
            }
        }
        public async Task PopulateSchemeExcludeMappingHistory()
        {
            try
            {
                SchemeExcludeGridViewRecords = await PopulateSchemeMappingHistory();
            }
            catch (Exception)
            {
                throw;
            }
        }
        public async Task OnFileUploadInsertIntoDB(List<SchemeExcludeMapping> schemeExcludeMappingRecords)
        {
            try
            {
                await InsertIntoDB(schemeExcludeMappingRecords);
            }
            catch (Exception ex)
            {

                throw;
            }
        }
        public async Task PageIndexChanged(int pageNumber)
        {
            PageNumber = pageNumber;
            await PopulateViewModel();
        }
        public async Task OnSorting(SortCriteria sortCriteria)
        {
            try
            {
                SortCriterias.Clear();
                SortCriterias.Add(sortCriteria);
                await PopulateViewModel();
            }
            catch (Exception ex)
            {
                throw ex;
            }

        }
        public async Task OnFilterApply(Dictionary<string, string> keyValuePairs)
        {
            try
            {
                FilterCriterias.Clear();
                foreach (var keyValue in keyValuePairs)
                {
                    if (!string.IsNullOrEmpty(keyValue.Value))
                    {
                        if (keyValue.Value.Contains(","))
                        {
                            string[] values = keyValue.Value.Split(',');
                            FilterCriterias.Add(new FilterCriteria(keyValue.Key, values, FilterType.In));
                        }
                        else if (keyValue.Key.Contains("Date"))
                        {
                            if (keyValue.Key == "StartDate")
                                FilterCriterias.Add(new FilterCriteria(keyValue.Key, _commonFunctions.GetDateOnlyInFormat(keyValue.Value), FilterType.GreaterThanOrEqual));
                            if (keyValue.Key == "EndDate")
                                FilterCriterias.Add(new FilterCriteria(keyValue.Key, _commonFunctions.GetDateOnlyInFormat(keyValue.Value), FilterType.LessThanOrEqual));
                        }
                        else if (keyValue.Key.Contains("IsActive"))
                        {
                            if (keyValue.Key.Equals("IsActive", StringComparison.OrdinalIgnoreCase) && keyValue.Value.Equals("True",StringComparison.OrdinalIgnoreCase))
                            {

                                FilterCriterias.Add(new FilterCriteria(keyValue.Key, 1, FilterType.Equal));
                            }
                            else
                            {
                                FilterCriterias.Add(new FilterCriteria(keyValue.Key, 0, FilterType.Equal));
                            }
                        }
                        else
                        {
                            FilterCriterias.Add(new FilterCriteria(keyValue.Key, keyValue.Value, FilterType.Like));
                        }
                    }
                }
                if (keyValuePairs.Any(kvp => kvp.Key.Equals("isactive", StringComparison.OrdinalIgnoreCase)
                            && kvp.Value.Equals("false", StringComparison.OrdinalIgnoreCase)))
                {
                    await PopulateSchemeExcludeMappingHistory();
                }
                else
                {
                    await PopulateViewModel();
                }
            }
            catch (Exception ex)
            {

            }
        }

        public abstract Task<List<ISchemeExcludeMapping>> PopulateUI();
        public abstract Task<List<ISchemeExcludeMapping>> PopulateSchemeMappingHistory();
        public abstract Task InsertIntoDB(List<SchemeExcludeMapping> schemeExcludeMappingRecords);
    }
}
