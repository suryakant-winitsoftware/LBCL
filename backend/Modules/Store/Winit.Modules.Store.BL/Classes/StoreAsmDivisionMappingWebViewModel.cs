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
using Newtonsoft.Json;
using Winit.Modules.Scheme.Model.Classes;
using Winit.Modules.Scheme.Model.Interfaces;
using Winit.Shared.Models.Common;
using Winit.Shared.Models.Enums;
using Winit.Modules.Store.Model.Interfaces;
using Winit.Modules.Store.Model.Classes;
using Winit.Shared.CommonUtilities.Common;

namespace Winit.Modules.Store.BL.Classes
{
    public class StoreAsmDivisionMappingWebViewModel : StoreAsmDivisionMappingBaseViewModel
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
        public StoreAsmDivisionMappingWebViewModel(IServiceProvider serviceProvider, IFilterHelper filter, ISortHelper sorter, IListHelper listHelper, IAppUser appUser, IAppSetting appSetting,
            IDataManager dataManager, Shared.Models.Common.IAppConfig appConfigs, ApiService apiService, CommonFunctions commonFunctions)
            : base(serviceProvider, filter, sorter, listHelper, appUser, appSetting, dataManager, appConfigs, apiService, commonFunctions)
        {
            _filter = filter;
            _sorter = sorter;
            _appUser = appUser;
            _listHelper = listHelper;
            _apiService = apiService;
            _appConfigs = appConfigs;
            _appSetting = appSetting;
        }
        public override async Task<List<IAsmDivisionMapping>> PopulateUI()
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
                $"{_appConfigs.ApiBaseUrl}AsmMapping/SelectAllStoreAsmMapping",
                HttpMethod.Post, pagingRequest);
            if (apiResponse != null && apiResponse.IsSuccess && apiResponse.Data != null)
            {
                ApiResponse<PagedResponse<Winit.Modules.Store.Model.Classes.AsmDivisionMapping>> pagedResponse = JsonConvert.DeserializeObject<ApiResponse<PagedResponse<Winit.Modules.Store.Model.Classes.AsmDivisionMapping>>>(apiResponse.Data);
                TotalItemsCount = pagedResponse.Data.TotalCount;
                return pagedResponse.Data.PagedData.ToList<Winit.Modules.Store.Model.Interfaces.IAsmDivisionMapping>();
            }
            return default;
        }

        public override async Task InsertIntoDB(List<StoreAsmMapping> schemeExcludeMappingRecords)
        {
            try
            {
                ApiResponse<string> apiResponseResult = await _apiService.FetchDataAsync(
                $"{_appConfigs.ApiBaseUrl}AsmMapping/BulkImportAsmMapping", HttpMethod.Post, schemeExcludeMappingRecords);

                if (apiResponseResult != null && apiResponseResult.IsSuccess && apiResponseResult.Data != null)
                {
                    StoreAsmMappingErrorRecords = JsonConvert.DeserializeObject<List<StoreAsmMapping>>(apiResponseResult.Data);
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
