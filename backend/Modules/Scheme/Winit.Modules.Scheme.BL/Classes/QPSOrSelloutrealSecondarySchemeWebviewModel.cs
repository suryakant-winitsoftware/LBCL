using Winit.Modules.Base.BL;
using Winit.Modules.Common.BL.Interfaces;
using Winit.Modules.Common.Model.Interfaces;
using Winit.Modules.Promotion.Model.Classes;
using Winit.Modules.Scheme.Model.Constants;
using Winit.Modules.Scheme.Model.Interfaces;
using Winit.Modules.Setting.BL.Interfaces;
using Winit.Modules.SKU.BL.Interfaces;
using Winit.Modules.User.Model.Constants;
using Winit.Shared.CommonUtilities.Common;
using Winit.Shared.Models.Common;
using Winit.UIComponents.Common;
using Winit.UIComponents.Common.Services;
using Winit.UIComponents.SnackBar;

namespace Winit.Modules.Scheme.BL.Classes
{
    public class QPSOrSelloutrealSecondarySchemeWebviewModel : QPSOrSelloutrealSecondarySchemeBaseviewModel
    {
        ApiService _apiService;
        IAppConfig _appConfig;

        public QPSOrSelloutrealSecondarySchemeWebviewModel(IDataManager dataManager,
            ApiService apiService, IServiceProvider serviceProvider,
            IAppConfig appConfig, CommonFunctions commonFunctions, ILoadingService loadingService, ISchemeSlab schemeSlab, IAppUser appUser,
        IToast toast, IAlertService alertService, IAppSetting appSetting, IAddProductPopUpDataHelper addProductPopUpDataHelper) : base(appConfig
                , apiService, serviceProvider, appUser, loadingService, alertService, appSetting, commonFunctions, addProductPopUpDataHelper, toast)
        {
            _dataManager = dataManager;
            _apiService = apiService;
            _appConfig = appConfig;
            //_commonFunctions = commonFunctions;
            //_loadingService = loadingService;
            //_appUser = appUser;
            //_serviceProvider = serviceProvider;

            PromoMasterView = new()
            {
                PromotionView = new(),
                PromoOrderViewList = new(),
                PromoOrderItemViewList = new(),
                PromoOfferViewList = new(),
                PromoOfferItemViewList = new(),
                PromoConditionViewList = new(),
            };
            SchemeProducts = new();
            SKUGroupDDL = new();
            SKUGroupTypeDDL = new();
            OfferTypeDDL = new();
            ListItems = new();
            SchemeSlab = schemeSlab;
            SchemeSlabs = new();
        }

        #region API Calling Methodes



        protected override async Task GetListItemsByCodes()
        {
            Winit.Modules.ListHeader.Model.Classes.ListItemRequest listItemRequest = new Winit.Modules.ListHeader.Model.Classes.ListItemRequest()
            {
                isCountRequired = true,
                Codes = new List<string>()
                {
                    "PROMO_INSTANT_ORDERTYPE",
                    "PROMO_INVOICE_OFFERTYPE"
                }
            };
            Shared.Models.Common.ApiResponse<PagedResponse<Winit.Modules.ListHeader.Model.Interfaces.IListItem>> apiResponse = await _apiService.FetchDataAsync<PagedResponse<Winit.Modules.ListHeader.Model.Interfaces.IListItem>>($"{_appConfig.ApiBaseUrl}ListItemHeader/GetListItemsByCodes", HttpMethod.Post, listItemRequest);
            if (apiResponse != null)
            {
                if (apiResponse.IsSuccess && apiResponse.StatusCode == 200)
                {
                    ListItems = new();
                    OrderTypeDDL = new();
                    OfferTypeDDL = new();
                    foreach (var item in apiResponse?.Data?.PagedData)
                    {

                        SelectionItem selectionItem = new SelectionItem
                        {
                            UID = item.UID,
                            Label = item.Name,
                            Code = item.Code,
                        };
                        if (item.ListHeaderUID.Equals("PROMO_INSTANT_ORDERTYPE"))
                        {
                            OrderTypeDDL.Add(selectionItem);
                        }
                        else if (item.ListHeaderUID.Equals("PROMO_INVOICE_OFFERTYPE"))
                        {
                            if (selectionItem.Code.Equals("foc", StringComparison.OrdinalIgnoreCase))
                            {
                                if (!IsQPSScheme)
                                    OfferTypeDDL.Add(selectionItem);
                            }
                            else
                            {
                                OfferTypeDDL.Add(selectionItem);

                            }
                        }
                        ListItems.Add(item);
                    }
                }
            }

        }

        public async Task<ApiResponse<string>> Save()
        {
            if (!IsApplicableToCustomersValidated())
            {
                return new ApiResponse<string>(data: null, statusCode: 500, errorMessage: "Please select any one in Applicable to customers");
            }

            SaveOrUpdateSlabTypePromotion_V1();
            if (IsNew)
            {
                PromoMasterView.ApprovalRequestItem = PrepareApprovalRequestItem(UserHierarchyTypeConst.Emp, _appUser.Emp.UID);
            }
            return await _apiService.FetchDataAsync($"{_appConfig.ApiBaseUrl}Promotion/CUDPromotionMaster", HttpMethod.Post, PromoMasterView);
        }

        public async Task<ApiResponse<string>> UpdateScheme()
        {

            return await _apiService.FetchDataAsync($"{_appConfig.ApiBaseUrl}Promotion/UpdatePromotion", HttpMethod.Put, PromoMasterView.PromotionView);
        }
        protected override async Task GetSchemeDetailsBYUID(string UID)
        {
            await GetPromotionDetailsBYUID(UID);
            FileSysList = await GetAttatchedFiles(UID);
            if (PromoMasterView != null && PromoMasterView.PromotionView != null)
            {

                //SetChannelPartnerSelectedonEditMode(PromoMasterView.PromotionView.OrgUID);
                //await GetSKUsByOrgUID(PromoMasterView.PromotionView.OrgUID);
                List<string> strings = new List<string>();
                if (PromoMasterView.PromoOrderItemViewList != null && PromoMasterView.PromoOrderItemViewList.Any(item => QpsConstants.SKU.Equals(item.ItemCriteriaType, StringComparison.OrdinalIgnoreCase)))
                {
                    strings.AddRange(PromoMasterView.PromoOrderItemViewList.Where(p => QpsConstants.SKU.Equals(p.ItemCriteriaSelected)).Select(p => p.ItemCriteriaSelected));
                }
                if (PromoMasterView.PromoOfferItemViewList != null && PromoMasterView.PromoOfferItemViewList.Any(item => QpsConstants.SKU.Equals(item.ItemCriteriaType, StringComparison.OrdinalIgnoreCase)))
                {
                    strings.AddRange(PromoMasterView.PromoOfferItemViewList.Where(p => QpsConstants.SKU.Equals(p.ItemCriteriaSelected)).Select(p => p.ItemCriteriaSelected));
                }
                PagingRequest pagingRequest = new PagingRequest()
                {
                    FilterCriterias = new() { new("UID",strings,Shared.Models.Enums.FilterType.In)
                    }
                };
                var skus = await GetAllSKUs(pagingRequest);
                SetEditMode(skus != null ? skus : default);
                SchemeSlabs = SchemeSlabs.OrderBy(p => p.Id).ToList();
                SchemeProducts = SchemeProducts.OrderBy(p => p.Id).ToList();

            }
        }

        private async Task GetPromotionDetailsBYUID(string UID)
        {
            Shared.Models.Common.ApiResponse<Winit.Modules.Promotion.Model.Classes.PromoMasterView> apiResponse = await _apiService.FetchDataAsync<Winit.Modules.Promotion.Model.Classes.PromoMasterView>(
                $"{_appConfig.ApiBaseUrl}Promotion/GetPromotionDetailsByUID?PromotionUID={UID}", HttpMethod.Get);
            if (apiResponse != null)
            {
                if (apiResponse.IsSuccess && apiResponse.StatusCode == 200 && apiResponse.Data != null)
                {
                    PromoMasterView = apiResponse.Data;
                    SetEditModeForApplicabletoCustomers(PromoMasterView.SchemeBranches,
                    PromoMasterView.SchemeOrgs, PromoMasterView.SchemeBroadClassifications);
                }
            }
        }
        public async Task<Shared.Models.Common.ApiResponse<string>> DeletePromoOrderItemsByUIDs(List<string> UIDs)
        {
            return await _apiService.FetchDataAsync(
                  $"{_appConfig.ApiBaseUrl}Promotion/DeletePromoOrderItemsByUIDs", HttpMethod.Post, UIDs);

        }
        public async Task<Shared.Models.Common.ApiResponse<string>> DeletePromotionSlabByPromoOrderUID(string promoOrderUID)
        {
            return await _apiService.FetchDataAsync(
                  $"{_appConfig.ApiBaseUrl}Promotion/DeletePromotionSlabByPromoOrderUID?promoOrderUID={promoOrderUID}", HttpMethod.Delete);

        }

        #endregion
    }
}
