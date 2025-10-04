using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Winit.Modules.Base.BL.Helper.Interfaces;
using Winit.Modules.Base.BL;
using Winit.Modules.Common.BL.Interfaces;
using Winit.Modules.Common.Model.Interfaces;
using Winit.Modules.Setting.BL.Interfaces;
using Winit.Modules.SKU.BL.Interfaces;
using Winit.UIComponents.Common;
using Winit.UIComponents.SnackBar;
using Winit.Shared.CommonUtilities.Common;
using Winit.Modules.SKU.BL.Classes;
using Winit.UIComponents.SnackBar.Services;
using Winit.Modules.Scheme.Model.Interfaces;
using Winit.Shared.Models.Common;
using iTextSharp.text;
using Newtonsoft.Json;
using Winit.Modules.Common.BL.Classes;
using Winit.Shared.Models.Enums;
using Winit.Modules.CollectionModule.Model.Classes;
using Winit.Modules.Scheme.Model.Classes;
using Nest;
using Winit.Modules.Store.Model.Classes;
using Winit.Modules.Location.Model.Interfaces;
using Dapper;
using Winit.Modules.FileSys.Model.Classes;

namespace Winit.Modules.Scheme.BL.Classes
{
    public class SchemeExcludeMappingWebViewModel : SchemeExcludeMappingBaseViewModel
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly IFilterHelper _filter;
        private readonly ISortHelper _sorter;
        private readonly IListHelper _listHelper;
        private readonly IAppUser _appUser;
        private readonly IAppSetting _appSetting;
        private readonly IDataManager _dataManager;
        private readonly Shared.Models.Common.IAppConfig _appConfigs;
        private readonly ApiService _apiService;
        public SchemeExcludeMappingWebViewModel(IServiceProvider serviceProvider, IFilterHelper filter, ISortHelper sorter, IListHelper listHelper, IAppUser appUser, IAppSetting appSetting,
            IDataManager dataManager, Shared.Models.Common.IAppConfig appConfigs, ApiService apiService, CommonFunctions commonFunctions)
            : base(serviceProvider, filter, sorter, listHelper, appUser, appSetting, dataManager, appConfigs, apiService, commonFunctions)
        {
            _serviceProvider = serviceProvider;
            _filter = filter;
            _sorter = sorter;
            _appUser = appUser;
            _listHelper = listHelper;
            _apiService = apiService;
            _appConfigs = appConfigs;
            _appSetting = appSetting;
        }

        public override async Task<List<ISchemeExcludeMapping>> PopulateUI()
        {
            PagingRequest pagingRequest = new PagingRequest();
            pagingRequest.FilterCriterias = new List<FilterCriteria>();
            pagingRequest.PageNumber = PageNumber;
            pagingRequest.PageSize = PageSize;
            pagingRequest.FilterCriterias = FilterCriterias;
            // pagingRequest.FilterCriterias.Add(new FilterCriteria("status", CurrentStatus, FilterType.Equal)); add more filter criteria
            pagingRequest.IsCountRequired = true;
            pagingRequest.SortCriterias = (SortCriterias == null || !SortCriterias.Any()) ? [DefaultSortCriteria] : SortCriterias;
            ApiResponse<string> apiResponse = await _apiService.FetchDataAsync(
                $"{_appConfigs.ApiBaseUrl}SchemeExcludeController/SelectAllSchemeExcludeMapping",
                HttpMethod.Post, pagingRequest);
            if (apiResponse != null && apiResponse.IsSuccess && apiResponse.Data != null)
            {
                ApiResponse<PagedResponse<Winit.Modules.Scheme.Model.Classes.SchemeExcludeMapping>> pagedResponse = JsonConvert.DeserializeObject<ApiResponse<PagedResponse<Winit.Modules.Scheme.Model.Classes.SchemeExcludeMapping>>>(apiResponse.Data);
                TotalItemsCount = pagedResponse.Data.TotalCount;
                return pagedResponse.Data.PagedData.ToList<Winit.Modules.Scheme.Model.Interfaces.ISchemeExcludeMapping>();
            }
            return default;
        }
        public override async Task<List<ISchemeExcludeMapping>> PopulateSchemeMappingHistory()
        {
            PagingRequest pagingRequest = new PagingRequest();
            pagingRequest.FilterCriterias = new List<FilterCriteria>();
            pagingRequest.PageNumber = PageNumber;
            pagingRequest.PageSize = PageSize;
            pagingRequest.FilterCriterias = FilterCriterias;
            // pagingRequest.FilterCriterias.Add(new FilterCriteria("status", CurrentStatus, FilterType.Equal)); add more filter criteria
            pagingRequest.IsCountRequired = true;
            pagingRequest.SortCriterias = (SortCriterias == null || !SortCriterias.Any()) ? [DefaultSortCriteria] : SortCriterias;
            ApiResponse<string> apiResponse = await _apiService.FetchDataAsync(
                $"{_appConfigs.ApiBaseUrl}SchemeExcludeController/SelectAllSchemeExcludeMappingHistory",
                HttpMethod.Post, pagingRequest);
            if (apiResponse != null && apiResponse.IsSuccess && apiResponse.Data != null)
            {
                ApiResponse<PagedResponse<Winit.Modules.Scheme.Model.Classes.SchemeExcludeMapping>> pagedResponse = JsonConvert.DeserializeObject<ApiResponse<PagedResponse<Winit.Modules.Scheme.Model.Classes.SchemeExcludeMapping>>>(apiResponse.Data);
                TotalItemsCount = pagedResponse.Data.TotalCount;
                return pagedResponse.Data.PagedData.ToList<Winit.Modules.Scheme.Model.Interfaces.ISchemeExcludeMapping>();
            }
            return default;
        }

        public override async Task InsertIntoDB(List<SchemeExcludeMapping> schemeExcludeMappingRecords)
        {
            try
            {
                schemeExcludeMappingRecords.ForEach(e =>
                {
                    e.SchemeType = "CashDiscount";
                    e.SchemeUID = "CashDiscount";
                    e.CreatedBy = _appUser.Emp.UID;
                    e.ModifiedBy = _appUser.Emp.UID;
                    e.CreatedTime = DateTime.Now;
                    e.ModifiedTime = DateTime.Now;
                    e.IsActive = true;
                });
                ApiResponse<string> apiResponseResult = await _apiService.FetchDataAsync(
                $"{_appConfigs.ApiBaseUrl}SchemeExcludeController/BulkImport", HttpMethod.Post, schemeExcludeMappingRecords);

                if (apiResponseResult != null && apiResponseResult.IsSuccess && apiResponseResult.Data != null)
                {
                    SchemeExcludeErrorRecords = JsonConvert.DeserializeObject<List<SchemeExcludeMapping>>(apiResponseResult.Data);
                }
                //return null;
            }
            catch (Exception ex)
            {
                throw;
            }
            finally
            {

            }
        }
    }
}

