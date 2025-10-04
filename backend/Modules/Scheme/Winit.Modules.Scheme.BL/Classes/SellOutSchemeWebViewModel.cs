using Winit.Modules.ApprovalEngine.Model.Classes;
using Winit.Modules.Base.BL;
using Winit.Modules.Base.BL.Helper.Interfaces;
using Winit.Modules.Common.BL.Interfaces;
using Winit.Modules.Common.Model.Interfaces;
using Winit.Modules.ListHeader.Model.Interfaces;
using Winit.Modules.Scheme.Model.Interfaces;
using Winit.Modules.Setting.BL.Interfaces;
using Winit.Modules.SKU.BL.Interfaces;
using Winit.Modules.SKU.Model.Classes;
using Winit.Modules.SKU.Model.Interfaces;
using Winit.Modules.User.Model.Constants;
using Winit.Shared.CommonUtilities.Common;
using Winit.Shared.Models.Common;
using Winit.UIComponents.Common;
using Winit.UIComponents.Common.Services;
using Winit.UIComponents.SnackBar;

namespace Winit.Modules.Scheme.BL.Classes
{
    public class SellOutSchemeWebViewModel : SellOutSchemeBaseViewModel
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
        public SellOutSchemeWebViewModel(IServiceProvider serviceProvider, IFilterHelper filter, ISortHelper sorter, IListHelper listHelper, IAppUser appUser, IAppSetting appSetting, IDataManager dataManager, Shared.Models.Common.IAppConfig appConfigs, ApiService apiService, ILoadingService loadingService, IAlertService alertService, CommonFunctions commonFunctions, IAddProductPopUpDataHelper addProductPopUpDataHelper, IToast toast) : base(serviceProvider, filter, sorter, listHelper, appUser, appSetting, dataManager, appConfigs, apiService, loadingService, alertService, commonFunctions, addProductPopUpDataHelper, toast)
        {
            _serviceProvider = serviceProvider;
            _filter = filter;
            _sorter = sorter;
            _listHelper = listHelper;
            _appUser = appUser;
            _dataManager = dataManager;
            _appConfigs = appConfigs;
            _apiService = apiService;
        }
        public override async Task<List<ISKUMaster>> GetSKUsMasterBySKUUIDs(SKUMasterRequest sKUMasterRequest)
        {
            try
            {
                ApiResponse<PagedResponse<ISKUMaster>> apiResponse =
                   await _apiService.FetchDataAsync<PagedResponse<ISKUMaster>>(
                   $"{_appConfig.ApiBaseUrl}SKU/GetAllSKUMasterData",
                   HttpMethod.Post, sKUMasterRequest);

                if (apiResponse != null && apiResponse.IsSuccess && apiResponse.Data != null && apiResponse.Data.PagedData != null)
                {
                    return apiResponse.Data.PagedData.ToList();
                }
            }
            catch (Exception)
            {
                throw;
            }
            return [];
        }
        protected override async Task GetSellOutReasons(string ListHeaderUID)
        {
            try
            {
                ApiResponse<IEnumerable<IListItem>> apiResponse = await _apiService.FetchDataAsync<IEnumerable<IListItem>>
                    ($"{_appConfig.ApiBaseUrl}ListItemHeader/GetListItemsByHeaderUID", HttpMethod.Post, ListHeaderUID);


                if (apiResponse != null && apiResponse.IsSuccess && apiResponse.Data != null && apiResponse.Data != null)
                {
                    SellOutReasons.Clear();
                    foreach (var s in apiResponse!.Data)
                    {
                        ISelectionItem selectionItem = new SelectionItem()
                        {
                            UID = s.UID,
                            Code = s.Code,
                            Label = s.Name,
                        };
                        SellOutReasons.Add(selectionItem);
                    }

                }
            }
            catch (Exception)
            {
                throw;
            }

        }

        public async Task<bool> SaveOrUpdate()
        {
            if (IsNew)
            {

                Save();
                SellOutMaster.ApprovalRequestItem = PrepareApprovalRequestItem();
            }
            else
            {
                Update();
            }

            ApiResponse<string> apiResponse = await _apiService.FetchDataAsync<string>(
                    $"{_appConfig.ApiBaseUrl}SellOutSchemeHeader/CrudSellOutMaster",
                    HttpMethod.Post, SellOutMaster);

            return apiResponse != null && apiResponse.IsSuccess;
        }
        private ApprovalRequestItem? PrepareApprovalRequestItem()
        {
            ApprovalRequestItem approvalRequestItem = new ApprovalRequestItem();
            approvalRequestItem.HierarchyType =UserHierarchyTypeConst.StoreBM;
            approvalRequestItem.HierarchyUid = SelectedChannelPartner!.UID;
            approvalRequestItem.RuleId = RuleId;
            approvalRequestItem.Payload = new Dictionary<string, object>
            {
                { "RequesterId", _appUser.Emp.Code },
                {
                    "UserRoleCode" , _appUser.Role.Code
                },
                { "Remarks", "Need approval" },
                { "Customer", new Customer { FirmType = UserType } }
            };

            return approvalRequestItem;
        }
        protected override async Task GetSellOutMasterByUID(string UID)
        {
            await GetSellOutSchemeByUID(UID);
            if (SellOutMaster is not null && SellOutMaster.SellOutSchemeHeader is not null)
            {
                SetChannelPartnerSelectedonEditMode(SellOutMaster.SellOutSchemeHeader.FranchiseeOrgUID);
                await GetSKUsByOrgUID(SellOutMaster.SellOutSchemeHeader.FranchiseeOrgUID);
                await GetSkusByOrgUID();
                SetEditMode();
            }

        }
        private async Task GetSellOutSchemeByUID(string UID)
        {

            ApiResponse<ISellOutMasterScheme> apiResponse = await _apiService.FetchDataAsync<ISellOutMasterScheme>(
                    $"{_appConfig.ApiBaseUrl}SellOutSchemeHeader/GetSellOutMasterByUID?uID={UID}",
                    HttpMethod.Get);

            if (apiResponse is not null && apiResponse.IsSuccess)
            {
                if (apiResponse.Data != null)
                {
                    SellOutMaster = apiResponse.Data;
                }
            }
        }

        public async Task<bool> DeleteSellOutLinesByUIDs(IEnumerable<string> purchaseOrderLineItems)
        {

            ApiResponse<string> apiResponse = await _apiService.FetchDataAsync(
                    $"{_appConfig.ApiBaseUrl}SellOutSchemeLine/DeleteSellOutLinesByUIDs",
                    HttpMethod.Delete);

            return apiResponse != null && apiResponse.IsSuccess;
        }

        public override Task ApproveSellOutSchemeHeaderItemAsync(string uid)
        {
            throw new NotImplementedException();
        }

        public override async Task<List<IPreviousOrders>> GetPreviousOrdersFromAPI(string UID)
        {
            try
            {
                ApiResponse<List<IPreviousOrders>> apiResponse =
                   await _apiService.FetchDataAsync<List<IPreviousOrders>>(
                   $"{_appConfigs.ApiBaseUrl}SellOutSchemeLine/GetPreviousOrdersByChannelPartnerUID?UID={UID}",
                   HttpMethod.Get);

                if (apiResponse != null && apiResponse.IsSuccess && apiResponse.Data != null)
                {
                    return apiResponse.Data.ToList();
                }
            }
            catch (Exception)
            {
                throw;
            }
            return [];
        }


    }
}
