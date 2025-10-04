using Newtonsoft.Json;
using Winit.Modules.Base.BL.Helper.Interfaces;
using Winit.Modules.Common.BL.Interfaces;
using Winit.Modules.Common.Model.Interfaces;
using Winit.Modules.Setting.BL.Interfaces;
using Winit.Modules.SKU.Model.Classes;
using Winit.Modules.SKU.Model.Interfaces;
using Winit.Shared.Models.Common;
using Winit.Shared.Models.Enums;
using Microsoft.Extensions.Logging;

namespace Winit.Modules.SKU.BL.Classes
{
    /// <summary>
    /// Web view model for maintaining SKU functionality
    /// </summary>
    public class MaintainSKUWebViewModel : MaintainSKUBaseViewModel
    {
        private readonly ILogger<MaintainSKUWebViewModel> _logger;

        /// <summary>
        /// Initializes a new instance of the MaintainSKUWebViewModel class
        /// </summary>
        public MaintainSKUWebViewModel(
            IServiceProvider serviceProvider,
            IFilterHelper filter,
            ISortHelper sorter,
            IListHelper listHelper,
            IAppUser appUser,
            IAppSetting appSetting,
            IDataManager dataManager,
            IAppConfig appConfigs,
            Base.BL.ApiService apiService,
            ILogger<MaintainSKUWebViewModel> logger) 
            : base(serviceProvider, filter, sorter, listHelper, appUser, appSetting, dataManager, appConfigs, apiService, logger)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Populates the view model with data
        /// </summary>
        public override async Task PopulateViewModel()
        {
            try
            {
                await base.PopulateViewModel();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while populating view model");
                throw;
            }
        }

        /// <summary>
        /// Gets the maintain SKU grid data
        /// </summary>
        public override async Task GetMaintainSKUGridData()
        {
            try
            {
                await GetMaintainSKUGridDataFromAPIAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while getting maintain SKU grid data");
                throw;
            }
        }

        /// <summary>
        /// Deletes a SKU by its UID
        /// </summary>
        public override async Task<string> DeleteSKU(string uid)
        {
            if (string.IsNullOrEmpty(uid))
            {
                throw new ArgumentNullException(nameof(uid));
            }

            try
            {
                return await DeleteSKUDataFromAPIAsync(uid);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while deleting SKU");
                throw;
            }
        }

        /// <summary>
        /// Gets the SKU attribute type
        /// </summary>
        public override async Task<ISKUAttributeLevel> GetSKUAttributeType()
        {
            try
            {
                return await GetSKUAttributeTypeFromAPIAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while getting SKU attribute type");
                throw;
            }
        }

        private async Task GetMaintainSKUGridDataFromAPIAsync()
        {
            try
            {
                var pagingRequest = new PagingRequest
                {
                    PageNumber = PageNumber,
                    PageSize = PageSize,
                    FilterCriterias = SKUFilterCriterias,
                    SortCriterias = new List<SortCriteria>
                    {
                        new SortCriteria("SKUModifiedTime", SortDirection.Desc)
                    },
                    IsCountRequired = true
                };

                var isActiveFilter = pagingRequest.FilterCriterias.Find(e => "IsActive".Equals(e.Name, StringComparison.OrdinalIgnoreCase));
                if (isActiveFilter == null)
                {
                    pagingRequest.FilterCriterias.Add(new FilterCriteria("IsActive", 1, FilterType.Equal));
                }

                var apiResponse = await _apiService.FetchDataAsync(
                    $"{_appConfigs.ApiBaseUrl}SKU/SelectAllSKUDetailsWebView",
                    HttpMethod.Post,
                    pagingRequest);

                MaintainSKUGridList.Clear();

                if (apiResponse?.IsSuccess == true && apiResponse.Data != null)
                {
                    var pagedResponse = JsonConvert.DeserializeObject<ApiResponse<PagedResponse<SKUListView>>>(apiResponse.Data);
                    if (pagedResponse?.IsSuccess == true)
                    {
                        TotalSKUItemsCount = pagedResponse.Data.TotalCount;
                        MaintainSKUGridList.AddRange(pagedResponse.Data.PagedData);
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while getting maintain SKU grid data from API");
                throw;
            }
        }

        private async Task<string> DeleteSKUDataFromAPIAsync(string uid)
        {
            try
            {
                var apiResponse = await _apiService.FetchDataAsync(
                    $"{_appConfigs.ApiBaseUrl}SKU/DeleteSKU?UID={uid}",
                    HttpMethod.Delete,
                    uid);

                if (apiResponse?.IsSuccess == true && apiResponse.Data != null)
                {
                    return "SKU successfully deleted.";
                }

                if (apiResponse?.Data != null)
                {
                    var data = JsonConvert.DeserializeObject<ApiResponse<string>>(apiResponse.Data);
                    return $"Error failed to delete customers. Error: {data?.ErrorMessage}";
                }

                return "An unexpected error occurred.";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while deleting SKU data from API");
                throw;
            }
        }

        private async Task<ISKUAttributeLevel> GetSKUAttributeTypeFromAPIAsync()
        {
            try
            {
                var apiResponse = await _apiService.FetchDataAsync(
                    $"{_appConfigs.ApiBaseUrl}DataPreparation/GetAllSKUAttributeDDL",
                    HttpMethod.Get);

                if (apiResponse?.IsSuccess == true && apiResponse.Data != null)
                {
                    var response = JsonConvert.DeserializeObject<ApiResponse<SKUAttributeLevelDTO>>(apiResponse.Data);
                    if (response?.IsSuccess == true && response.Data != null)
                    {
                        var skuData = response.Data;
                        var skuAttributeLevel = new SKUAttributeLevel
                        {
                            SKUGroupTypes = skuData.SKUGroupTypes?.Cast<ISelectionItem>().ToList(),
                            SKUGroups = skuData.SKUGroups?.ToDictionary(
                                kvp => kvp.Key,
                                kvp => kvp.Value.Cast<ISelectionItem>().ToList())
                        };
                        return skuAttributeLevel;
                    }
                }
                return null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while getting SKU attribute type from API");
                throw;
            }
        }
    }
}
