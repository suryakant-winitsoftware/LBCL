using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Winit.Modules.Base.BL.Helper.Interfaces;
using Winit.Modules.Common.BL.Interfaces;
using Winit.Modules.Location.Model.Interfaces;
using Winit.Shared.Models.Common;
using Winit.Modules.CreditLimit.Model.Interfaces;
using Winit.Modules.Store.Model.Interfaces;
using Nest;
using System.Security.Cryptography;
using Winit.Modules.Org.Model.Interfaces;
using Winit.Modules.Setting.BL.Interfaces;
using Winit.Modules.Store.Model.Classes;
namespace Winit.Modules.CreditLimit.BL.Classes
{
    public class MaintainTemporaryCreditEnhancementWebViewModel : MaintainTemporaryCreditEnhancementBaseViewModel
    {
        private IServiceProvider _serviceProvider;
        private readonly IFilterHelper _filter;
        private readonly ISortHelper _sorter;
        private readonly IListHelper _listHelper;
        private Winit.Modules.Common.BL.Interfaces.IAppUser _appUser;
        private Winit.Shared.Models.Common.IAppConfig _appConfigs;
        private Winit.Modules.Base.BL.ApiService _apiService;
        private List<string> _propertiesToSearch = new List<string>();


        public MaintainTemporaryCreditEnhancementWebViewModel(IServiceProvider serviceProvider,
            IFilterHelper filter,
            ISortHelper sorter,
            IAppUser appUser,
            IListHelper listHelper, Winit.Shared.Models.Common.IAppConfig appConfigs,
            Winit.Modules.Base.BL.ApiService apiService,
            IAppSetting appSettings
            ) : base(serviceProvider, filter, sorter, appUser, listHelper, appConfigs, apiService, appSettings)
        {
            _serviceProvider = serviceProvider;
            _filter = filter;
            _sorter = sorter;
            _appUser = appUser;
            _listHelper = listHelper;
            _apiService = apiService;
            _appConfigs = appConfigs;
            _propertiesToSearch.Add("Code");
            _propertiesToSearch.Add("Name");
        }

        public override async Task<List<Winit.Modules.CreditLimit.Model.Interfaces.ITemporaryCredit>> GetTemporaryCreditEnhancementDetailsData()
        {
            return await GetTemporaryCreditEnhancementDetailsDataFromAPIAsync();
        }
        public override async Task<List<IStore>> GetChannelPartnersListForTempCreditEnhancement()
        {
            return await GetChannelPartnersListForTempCreditEnhancementFromAPIAsync();
        }
        public override async Task<List<IOrg>> GetDivisionsListForCreditEnhancement(string UID)
        {
            return await GetDivisionsListForCreditEnhancementFromApiAsync(UID);
        }
        public override async Task<bool> SaveTemporaryCreditRequestAsync(ITemporaryCredit temporaryCreditEnhancementDetails)
        {
            return await SaveTemporaryCreditRequestDetailsAsync(temporaryCreditEnhancementDetails);
        }
        public override async Task<bool> SaveTemporaryCreditForOracle(ITemporaryCredit temporaryCreditEnhancementDetails)
        {
            return await SaveTemporaryCreditForOracleAsync(temporaryCreditEnhancementDetails);
        }
        public override async Task<ITemporaryCredit> GetTemporaryCreditRequestDetailsUID(string UID)
        {
            return await GetTemporaryCreditRequestDetailsUIDFromAPIAsync(UID);
        }
        public override async Task<List<IStoreCreditLimit>> GetCreditLimitsByChannelPartnerAndDivisionUID(string? storeUID, string divisionOrgUID)
        {
            return await GetCreditLimitsByChannelPartnerAndDivisionUIDApiAsync(storeUID, divisionOrgUID);
        }

        private async Task<List<IStoreCreditLimit>> GetCreditLimitsByChannelPartnerAndDivisionUIDApiAsync(string? storeUID, string divisionOrgUID)
        {
            try
            {
                StoreCreditLimitRequest storeCreditLimitRequest = new StoreCreditLimitRequest
                {
                    StoreUids = [storeUID],
                    DivisionUID = divisionOrgUID
                };

                Winit.Shared.Models.Common.ApiResponse<List<IStoreCreditLimit>> apiResponse = await
                    _apiService.FetchDataAsync<List<IStoreCreditLimit>>
                        ($"{_appConfigs.ApiBaseUrl}StoreCredit/GetCurrentLimitByStoreAndDivision", HttpMethod.Post, storeCreditLimitRequest);
                if (apiResponse != null && apiResponse.IsSuccess && apiResponse.Data is not null && apiResponse.Data.Any())
                {
                    return apiResponse.Data;
                }
                return default;
            }
            catch (Exception)
            {

                throw;
            }
        }

        private async Task<ITemporaryCredit> GetTemporaryCreditRequestDetailsUIDFromAPIAsync(string uID)
        {
            try
            {
                ApiResponse<Winit.Modules.CreditLimit.Model.Classes.TemporaryCredit> apiResponse =
                    await _apiService.FetchDataAsync<Winit.Modules.CreditLimit.Model.Classes.TemporaryCredit>(
                    $"{_appConfigs.ApiBaseUrl}TemporaryCredit/GetTemporaryCreditByUID/{uID}",
                    HttpMethod.Get);
                if (apiResponse != null && apiResponse.IsSuccess && apiResponse.Data != null)
                {
                    return apiResponse.Data;
                }
            }
            catch (Exception ex)
            {
                // Handle exceptions
                // Handle exceptions
            }
            return null;



        }

        private async Task<bool> SaveTemporaryCreditRequestDetailsAsync(ITemporaryCredit temporaryCreditEnhancementDetails)
        {
            try
            {
                ApiResponse<string> apiResponse;
                temporaryCreditEnhancementDetails.OrderNumber = DateTime.Now.ToString("yyyyddMMHHmmss");
                //temporaryCreditEnhancementDetails.UID = Guid.NewGuid().ToString();
                temporaryCreditEnhancementDetails.CreatedBy = _appUser.Emp.CreatedBy;
                temporaryCreditEnhancementDetails.ModifiedBy = _appUser.Emp.ModifiedBy;
                temporaryCreditEnhancementDetails.CreatedTime = DateTime.Now;
                temporaryCreditEnhancementDetails.ModifiedTime = DateTime.Now;
                temporaryCreditEnhancementDetails.ServerModifiedTime = DateTime.Now;
                temporaryCreditEnhancementDetails.ServerAddTime = DateTime.Now;
                temporaryCreditEnhancementDetails.SS = 0;
                temporaryCreditEnhancementDetails.Status = "Pending";
                apiResponse = await _apiService.FetchDataAsync(
                $"{_appConfigs.ApiBaseUrl}TemporaryCredit/CreateTemporaryCreditDetails",
                HttpMethod.Post, temporaryCreditEnhancementDetails);


                if (apiResponse != null && apiResponse.IsSuccess && apiResponse.Data != null)
                {
                    return true;
                }
                return false;
            }
            catch (Exception)
            {
                return false;
                // Handle exceptions
                // Handle exceptions
            }
        }

        private async Task<bool> SaveTemporaryCreditForOracleAsync(ITemporaryCredit temporaryCreditEnhancementDetails)
        {
            Winit.Modules.Int_CommonMethods.Model.Interfaces.IPendingDataRequest pagingRequest = new Modules.Int_CommonMethods.Model.Classes.PendingDataRequest();
            try
            {
                ApiResponse<string> apiResponse;
                pagingRequest.LinkedItemUid = temporaryCreditEnhancementDetails.UID;
                pagingRequest.Status = "Pending";
                pagingRequest.LinkedItemType = "TemporaryCreditLimit";
                pagingRequest.CreatedTime = DateTime.Now;
                pagingRequest.ModifiedTime = DateTime.Now;
                pagingRequest.CreatedBy = _appUser.Emp.CreatedBy;
                pagingRequest.ModifiedBy = _appUser.Emp.ModifiedBy;
                apiResponse = await _apiService.FetchDataAsync(
                $"{_appConfigs.ApiBaseUrl}IntPendingDataInsertion/InsertPendingData",
                HttpMethod.Post, pagingRequest);

                if (apiResponse != null && apiResponse.IsSuccess && apiResponse.Data != null)
                {
                    return true;
                }
                return false;
            }
            catch (Exception ex)
            {
                return false;
                // Handle exceptions
                // Handle exceptions
            }
        }

        private async Task<List<IStore>> GetChannelPartnersListForTempCreditEnhancementFromAPIAsync()
        {
            try
            {

                ApiResponse<List<Winit.Modules.Store.Model.Classes.Store>> apiResponse = await _apiService.FetchDataAsync<List<Winit.Modules.Store.Model.Classes.Store>>(
                $"{_appConfigs.ApiBaseUrl}store/GetChannelPartner?jobPositionUid={_appUser.SelectedJobPosition.UID}",
                HttpMethod.Get);
                if (apiResponse != null && apiResponse.IsSuccess && apiResponse.Data != null)
                {
                    return apiResponse.Data.ToList<IStore>();
                }
            }
            catch (Exception ex)
            {

            }
            return null;
        }

        private async Task<List<IOrg>> GetDivisionsListForCreditEnhancementFromApiAsync(string UID)
        {
            try
            {
                ApiResponse<List<Winit.Modules.Org.Model.Classes.Org>> apiResponse = await _apiService.FetchDataAsync<List<Winit.Modules.Org.Model.Classes.Org>>(
                $"{_appConfigs.ApiBaseUrl}Org/GetDivisions",
                HttpMethod.Post, UID);
                if (apiResponse != null && apiResponse.IsSuccess && apiResponse.Data != null)
                {
                    return apiResponse.Data.ToList<IOrg>();
                }
            }
            catch (Exception ex)
            {

            }
            return null;
        }

        private async Task<List<ITemporaryCredit>> GetTemporaryCreditEnhancementDetailsDataFromAPIAsync()
        {
            try
            {
                PagingRequest pagingRequest = new PagingRequest();
                pagingRequest.IsCountRequired = true;
                pagingRequest.FilterCriterias = FilterCriterias;
                pagingRequest.SortCriterias = SortCriterias;
                pagingRequest.PageSize = PageSize;
                pagingRequest.PageNumber = PageNo;

                ApiResponse<string> apiResponse = await _apiService.FetchDataAsync(
                $"{_appConfigs.ApiBaseUrl}TemporaryCredit/SelectTemporaryCreditDetails/{_appUser.SelectedJobPosition.UID}",
                HttpMethod.Post, pagingRequest);
                if (apiResponse != null && apiResponse.IsSuccess && apiResponse.Data != null)
                {
                    ApiResponse<PagedResponse<Winit.Modules.CreditLimit.Model.Classes.TemporaryCredit>> pagedResponse = JsonConvert.DeserializeObject<ApiResponse<PagedResponse<Winit.Modules.CreditLimit.Model.Classes.TemporaryCredit>>>(apiResponse.Data);
                    TotalCount = pagedResponse.Data.TotalCount;
                    return pagedResponse.Data.PagedData.OfType<Winit.Modules.CreditLimit.Model.Interfaces.ITemporaryCredit>().ToList();
                }
            }
            catch (Exception ex)
            {

            }
            return null;
        }
        public override async Task<List<IStore>?> GetChannelPartners()
        {
            ApiResponse<List<IStore>> apiResponse = await _apiService.FetchDataAsync<List<IStore>>(
            $"{_appConfigs.ApiBaseUrl}Store/GetChannelPartner?jobPositionUid={_appUser.SelectedJobPosition.UID}",
            HttpMethod.Get);

            return apiResponse != null && apiResponse.Data != null ? apiResponse.Data : null;
        }
    }

}
