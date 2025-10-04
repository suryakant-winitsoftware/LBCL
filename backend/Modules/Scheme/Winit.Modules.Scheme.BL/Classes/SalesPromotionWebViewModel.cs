using Winit.Modules.ApprovalEngine.Model.Classes;
using Winit.Modules.Base.BL;
using Winit.Modules.Base.BL.Helper.Interfaces;
using Winit.Modules.Common.BL.Interfaces;
using Winit.Modules.Common.Model.Interfaces;
using Winit.Modules.ListHeader.Model.Classes;
using Winit.Modules.Scheme.Model.Classes;
using Winit.Modules.Scheme.Model.Constants;
using Winit.Modules.Scheme.Model.Interfaces;
using Winit.Modules.Setting.BL.Interfaces;
using Winit.Modules.SKU.BL.Interfaces;
using Winit.Modules.User.Model.Constants;
using Winit.Shared.CommonUtilities.Common;
using Winit.Shared.CommonUtilities.Extensions;
using Winit.Shared.Models.Common;
using Winit.Shared.Models.Enums;
using Winit.UIComponents.Common;
using Winit.UIComponents.Common.Services;
using Winit.UIComponents.SnackBar;

namespace Winit.Modules.Scheme.BL.Classes
{
    public class SalesPromotionWebViewModel : SalesPromotionSchemeBaseViewModel
    {

        public SalesPromotionWebViewModel(IAppConfig appConfig, ApiService apiService, IServiceProvider serviceProvider,
            IFilterHelper filter, ISortHelper sorter, IListHelper listHelper, IAppUser appUser, IAppSetting appSetting,
            IDataManager dataManager, ILoadingService loadingService, IAlertService alertService, CommonFunctions commonFunctions, IAddProductPopUpDataHelper addProductPopUpDataHelper,
            IToast toast) : base(appConfig, apiService,
                serviceProvider, filter, sorter, listHelper, appUser, appSetting, dataManager,
                loadingService, alertService, commonFunctions, addProductPopUpDataHelper, toast)
        {
        }




        public override async Task<bool> UpdateSalesPromotion(SalesPromotionSchemeApprovalDTO salesPromotionSchemeApprovalDTO)
        {
            salesPromotionSchemeApprovalDTO.SalesPromotion.Status=SchemeConstants.Approved;
            AddUpdateFields(salesPromotionSchemeApprovalDTO.SalesPromotion);
            bool result = await UpdateSalesPromotionScheme(salesPromotionSchemeApprovalDTO);
            if (result)
            {
                //IsApproveMode = true;
                IsViewMode = true;
                return true;
            }
            return false;
        }

        public override async Task DeleteSalesPromotion()
        {
            // Implement delete logic using SalesPromotion
        }

        protected override async Task LoadSalesPromotion()
        {
            SalesPromotion = await GetSalesPromotionSchemeByUID(PromotionUID);
            FileSysList = await GetAttatchedFiles(PromotionUID);
            SetEditMode();
        }

        public override async Task<bool> ApproveSalesPromotion(SalesPromotionSchemeApprovalDTO salesPromotionSchemeApprovalDTO)
        {
            salesPromotionSchemeApprovalDTO.SalesPromotion.Status = SchemeConstants.Approved;
            AddUpdateFields(salesPromotionSchemeApprovalDTO.SalesPromotion);
            bool result = await UpdateSalesPromotionScheme(salesPromotionSchemeApprovalDTO);
            if (result)
            {
                ApprovedFiles.AddRange(FileSysList.FindAll(p => p.FileSysType == SchemeConstants.SalesPromotion));
                //IsApproveMode = true;
                IsViewMode = true;
                return true;
            }
            return false;
        }

        public override async Task<bool> RejectSalesPromotion(SalesPromotionSchemeApprovalDTO salesPromotionSchemeApprovalDTO)
        {
            salesPromotionSchemeApprovalDTO.SalesPromotion.Status = SchemeConstants.Rejected;
            AddUpdateFields(salesPromotionSchemeApprovalDTO.SalesPromotion);
            bool result = await UpdateSalesPromotionScheme(salesPromotionSchemeApprovalDTO);
            if (result)
            {
                //IsApproveMode = false;
                IsViewMode = false;
                return true;
            }
            return false;
        }


        #region Api Calling Methodes
        public override async Task GetListItem()
        {
            Winit.Modules.ListHeader.Model.Classes.ListItemRequest listItemRequest = new ListItemRequest()
            {
                Codes = ["ActivityType"]
            };

            ApiResponse<PagedResponse<Winit.Modules.ListHeader.Model.Interfaces.IListItem>> apiResponse = await _apiService.FetchDataAsync<PagedResponse<Winit.Modules.ListHeader.Model.Interfaces.IListItem>>($"{_appConfig.ApiBaseUrl}ListItemHeader/GetListItemsByCodes", HttpMethod.Post, listItemRequest);
            if (apiResponse is not null && apiResponse.IsSuccess)
            {
                if (apiResponse.Data is not null && apiResponse.Data.PagedData is not null)
                {
                    SetActivityType(apiResponse.Data.PagedData.ToList());
                }
            }


        }
        public override async Task<bool> CreateSalesPromotion()
        {
            // Implement create logic using SalesPromotion
            if (SelectedChannelPartner == null || _appUser.Emp == null || _appUser.SelectedJobPosition == null)
            {
                throw new CustomException(ExceptionStatus.Failed, "Required information is missing.");
            }
            SalesPromotion.EmpUID = _appUser.Emp.UID;
            SalesPromotion.UID = PromotionUID;
            SalesPromotion.Status = SchemeConstants.Pending;
            SalesPromotion.JobPositionUID = _appUser.SelectedJobPosition.UID;
            SalesPromotion.OrgUID = _appUser.SelectedJobPosition.OrgUID;
            SalesPromotion.FranchiseeOrgUID = SelectedChannelPartner.UID;
            SalesPromotion.AvailableProvision2Amount = Branch_P2Amount;
            SalesPromotion.AvailableProvision3Amount = HO_P3Amount;
            SalesPromotionSchemeApprovalDTO salesPromotionSchemeApprovalDTO = new SalesPromotionSchemeApprovalDTO();
            AddCreateFields(SalesPromotion);
            if (IsNew)
            {
                salesPromotionSchemeApprovalDTO.ApprovalRequestItem = PrepareApprovalRequestItem();
            }
            salesPromotionSchemeApprovalDTO.SalesPromotion=SalesPromotion;
            ApiResponse<string> apiResponse = await _apiService.FetchDataAsync(
                $"{_appConfig.ApiBaseUrl}SalesPromotionScheme/CreateSalesPromotionScheme",
                HttpMethod.Post, salesPromotionSchemeApprovalDTO);
            if (apiResponse != null)
            {
                if (apiResponse.IsSuccess)
                {
                    return true;
                }
                else
                {
                    await _alertService.ShowErrorAlert("Error", $"{apiResponse.Data}...{apiResponse.ErrorMessage}");
                }
            }

            return false;
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
        public async Task<ISalesPromotionScheme?> GetSalesPromotionSchemeByUID(string UID)
        {

            ApiResponse<ISalesPromotionScheme> apiResponse = await _apiService.FetchDataAsync<ISalesPromotionScheme>(
                    $"{_appConfig.ApiBaseUrl}SalesPromotionScheme/GetSalesPromotionSchemeByUID?UID={UID}",
                    HttpMethod.Get);

            return apiResponse != null && apiResponse.Data != null ? apiResponse.Data : _serviceProvider.CreateInstance<ISalesPromotionScheme>();
        }

        protected async Task<bool> UpdateSalesPromotionScheme(SalesPromotionSchemeApprovalDTO salesPromotionSchemeApprovalDTO)
        {
            try
            {
                ApiResponse<string> apiResponse = await _apiService.FetchDataAsync<string>(
                $"{_appConfig.ApiBaseUrl}SalesPromotionScheme/UpdateSalesPromotionScheme",
                HttpMethod.Put, salesPromotionSchemeApprovalDTO);

                return apiResponse != null && apiResponse.IsSuccess;
            }
            catch (Exception ex)
            {
                throw;
            }
        }
        protected override async Task GetAllSalesPromotions()
        {
            try
            {
                Winit.Shared.Models.Common.ApiResponse<PagedResponse<ISalesPromotionScheme>> apiResponse = await _apiService.FetchDataAsync<PagedResponse<ISalesPromotionScheme>>(
                        $"{_appConfig.ApiBaseUrl}SalesPromotionScheme/SelectAllSalesPromotionScheme",
                        HttpMethod.Post, PagingRequest);
                if (apiResponse != null && apiResponse.IsSuccess && apiResponse.Data != null && apiResponse.Data.PagedData is not null)
                {
                    SalesPromotions = apiResponse.Data.PagedData.ToList();
                    SalesPromotions.ForEach(p => p.ChannelPartnerName = ChannelPartner.Find(n => n.UID == p.FranchiseeOrgUID)?.Label ?? "NA");
                }
            }
            catch (Exception ex)
            {
                throw;
            }

        }
        #endregion
    }
}
