using Newtonsoft.Json;
using System.Linq;
using Winit.Modules.ApprovalEngine.BL.Classes;
using Winit.Modules.ApprovalEngine.Model.Classes;
using Winit.Modules.ApprovalEngine.Model.Interfaces;
using Winit.Modules.Base.BL;
using Winit.Modules.Common.BL.Interfaces;
using Winit.Modules.Email.Model.Classes;
using Winit.Modules.Int_CommonMethods.Model.Interfaces;
using Winit.Modules.Notification.Model.Classes;
using Winit.Modules.Notification.Model.Constant;
using Winit.Modules.Notification.Model.Interfaces;
using Winit.Modules.Org.Model.Interfaces;
using Winit.Modules.PurchaseOrder.BL.Interfaces;
using Winit.Modules.PurchaseOrder.Model.Classes;
using Winit.Modules.PurchaseOrder.Model.Interfaces;
using Winit.Modules.Scheme.Model.Classes;
using Winit.Modules.Scheme.Model.Interfaces;
using Winit.Modules.Setting.BL.Interfaces;
using Winit.Modules.SKU.Model.Classes;
using Winit.Modules.SKU.Model.Interfaces;
using Winit.Modules.SKU.Model.UIClasses;
using Winit.Modules.SMS.Model.Classes;
using Winit.Modules.Store.Model.Classes;
using Winit.Modules.Store.Model.Interfaces;
using Winit.Modules.User.Model.Classes;
using Winit.Modules.User.Model.Interface;
using Winit.Shared.Models.Common;
using Winit.Shared.Models.Enums;
using static System.Net.WebRequestMethods;
using AllApprovalRequest = Winit.Modules.ApprovalEngine.Model.Classes.AllApprovalRequest;

namespace Winit.Modules.PurchaseOrder.BL.Classes;

public class PurchaseOrderDataWebHelper : IPurchaseOrderDataHelper
{
    private IAppUser _appUser { get; }
    private IAppSetting _appSetting { get; }
    private IAppConfig _appConfigs { get; }
    private ApiService _apiService { get; }
    public Dictionary<string, List<string>> UserRole_Code { get; set; }
    private readonly JsonSerializerSettings _jsonSerializerSettings;
    public PurchaseOrderDataWebHelper(
        IAppUser appUser,
        IAppSetting appSetting,
        IAppConfig appConfigs,
        ApiService apiService,
        JsonSerializerSettings jsonSerializerSettings)
    {
        _appUser = appUser;
        _appSetting = appSetting;
        _appConfigs = appConfigs;
        _apiService = apiService;
        _jsonSerializerSettings = jsonSerializerSettings;
    }

    public async Task<List<Winit.Modules.Org.Model.Interfaces.IOrg>?> GetWareHouseData(string orgTypeUID,
        string parentUID)
    {
        try
        {
            ApiResponse<IEnumerable<Winit.Modules.Org.Model.Classes.Org>> apiResponse =
                await _apiService.FetchDataAsync<IEnumerable<Winit.Modules.Org.Model.Classes.Org>>(
                    $"{_appConfigs.ApiBaseUrl}Org/GetOrgByOrgTypeUID?OrgTypeUID={orgTypeUID}&parentUID={parentUID}",
                    HttpMethod.Get);
            if (apiResponse != null && apiResponse.IsSuccess && apiResponse.Data != null)
            {
                return apiResponse.Data.ToList<IOrg>();
            }
        }
        catch (Exception)
        {
            throw;
        }

        return null;
    }

    public async Task<List<SKUAttributeDropdownModel>> GetSKUAttributeDropDownData()
    {
        try
        {
            ApiResponse<List<SKUAttributeDropdownModel>> apiResponse =
                await _apiService.FetchDataAsync<List<SKUAttributeDropdownModel>>(
                    $"{_appConfigs.ApiBaseUrl}SKUAttributes/GetSKUGroupTypeForSKuAttribute",
                    HttpMethod.Get);

            if (apiResponse != null && apiResponse.IsSuccess && apiResponse.Data != null)
            {
                return apiResponse.Data;
            }
        }
        catch (Exception)
        {
            throw;
        }

        return [];
    }

    public async Task<List<SKUGroupSelectionItem>> GetSKUGroupSelectionItemBySKUGroupTypeUID(string? skuGroupTypeUID,
        string? parentUID)
    {
        try
        {
            ApiResponse<List<SKUGroupSelectionItem>> apiResponse =
                await _apiService.FetchDataAsync<List<SKUGroupSelectionItem>>(
                    $"{_appConfigs.ApiBaseUrl}SKUGroup/GetSKUGroupSelectionItemBySKUGroupTypeUID?skuGroupTypeUID={skuGroupTypeUID}&parentUID={parentUID}",
                    HttpMethod.Get);

            if (apiResponse != null && apiResponse.IsSuccess && apiResponse.Data != null)
            {
                return apiResponse.Data;
            }
        }
        catch (Exception)
        {
            throw;
        }

        return [];
    }

    public async Task<List<ISKUMaster>> GetSKUsMasterBySKUUIDs(SKUMasterRequest sKUMasterRequest)
    {
        try
        {
            ApiResponse<PagedResponse<ISKUMaster>> apiResponse =
                await _apiService.FetchDataAsync<PagedResponse<ISKUMaster>>(
                    $"{_appConfigs.ApiBaseUrl}SKU/GetAllSKUMasterData",
                    HttpMethod.Post, sKUMasterRequest);

            if (apiResponse != null && apiResponse.IsSuccess && apiResponse.Data != null &&
                apiResponse.Data.PagedData != null)
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

    public async Task<PagedResponse<ISKUPrice>> PopulatePriceMaster(List<SortCriteria>? sortCriterias = null,
        int? pageNumber = null, int? pageSize = null, List<FilterCriteria>? filterCriterias = null,
        bool? isCountRequired = null, string? type = null)
    {
        PagingRequest pagingRequest = new()
        {
            SortCriterias = sortCriterias,
            PageNumber = pageNumber ?? 0,
            PageSize = pageSize ?? 0,
            FilterCriterias = filterCriterias,
            IsCountRequired = isCountRequired ?? false
        };
        ApiResponse<PagedResponse<Winit.Modules.SKU.Model.Classes.SKUPrice>> apiResponse =
            await _apiService.FetchDataAsync<PagedResponse<Winit.Modules.SKU.Model.Classes.SKUPrice>>(
                $"{_appConfigs.ApiBaseUrl}SKUPrice/SelectAllSKUPriceDetails?type={type}",
                HttpMethod.Post, pagingRequest);

        return apiResponse != null && apiResponse.Data != null && apiResponse.Data.PagedData != null &&
               apiResponse.Data.PagedData.Any()
            ? new PagedResponse<ISKUPrice>
            {
                PagedData = apiResponse.Data.PagedData,
                TotalCount = apiResponse.Data.TotalCount
            }
            : new PagedResponse<ISKUPrice>();
    }

    public async Task<IStoreMaster?> GetStoreMasterByStoreUID(string storeUID)
    {
        ApiResponse<List<IStoreMaster>> apiResponse = await _apiService.FetchDataAsync<List<IStoreMaster>>(
            $"{_appConfigs.ApiBaseUrl}Store/GetStoreMastersByStoreUIDs",
            HttpMethod.Post, new List<string>
            {
                storeUID
            });

        return apiResponse != null && apiResponse.Data != null && apiResponse.Data.Any()
            ? apiResponse.Data.FirstOrDefault()
            : null;
    }

    public async Task<bool> SaveOrder(List<IPurchaseOrderMaster> purchaseOrderMasters,
        ApprovalRequestItem? approvalRequestItem = null)
    {
        ApiResponse<string> apiResponse = await _apiService.FetchDataAsync<string>(
            $"{_appConfigs.ApiBaseUrl}PurchaseOrder/CUD_PurchaseOrder",
            HttpMethod.Post, purchaseOrderMasters);

        return apiResponse != null && apiResponse.IsSuccess;
    }

    public async Task<bool> CreateMailRequest(List<string> OrderUids)
    {
        return default;
    }

    public async Task<List<IStore>?> GetChannelPartner(string jobPositionUid)
    {
        ApiResponse<List<IStore>> apiResponse = await _apiService.FetchDataAsync<List<IStore>>(
            $"{_appConfigs.ApiBaseUrl}Store/GetChannelPartner?jobPositionUid={jobPositionUid}",
            HttpMethod.Get);

        return apiResponse != null && apiResponse.Data != null ? apiResponse.Data : null;
    }

    public async Task<IPurchaseOrderMaster?> GetPurchaseOrderMasterByUID(string uID)
    {
        ApiResponse<IPurchaseOrderMaster> apiResponse = await _apiService.FetchDataAsync<IPurchaseOrderMaster>(
            $"{_appConfigs.ApiBaseUrl}PurchaseOrder/GetPurchaseOrderMasterByUID?uID={uID}",
            HttpMethod.Get);

        return apiResponse != null && apiResponse.Data != null ? apiResponse.Data : null;
    }

    public async Task<bool> DeletePurchaseOrderLinesByUIDs(IEnumerable<string> purchaseOrderLineItems)
    {
        ApiResponse<string> apiResponse = await _apiService.FetchDataAsync(
            $"{_appConfigs.ApiBaseUrl}PurchaseOrderLine/DeletePurchaseOrderLinesByUIDs",
            HttpMethod.Delete, purchaseOrderLineItems);

        return apiResponse != null && apiResponse.IsSuccess;
    }

    public async Task<List<ISelectionItem>?> GetProductOrgSelectionItems()
    {
        ApiResponse<List<ISelectionItem>> apiResponse = await _apiService.FetchDataAsync<List<ISelectionItem>>(
            $"{_appConfigs.ApiBaseUrl}Org/GetProductOrgSelectionItems",
            HttpMethod.Get);

        return apiResponse != null && apiResponse.IsSuccess ? apiResponse.Data : default;
    }

    public async Task<List<ISelectionItem>?> GetProductDivisionSelectionItems()
    {
        ApiResponse<List<ISelectionItem>> apiResponse = await _apiService.FetchDataAsync<List<ISelectionItem>>(
            $"{_appConfigs.ApiBaseUrl}Org/GetProductDivisionSelectionItems",
            HttpMethod.Get);

        return apiResponse != null && apiResponse.IsSuccess ? apiResponse.Data : default;
    }

    public async Task<List<string>?> GetOrgHierarchyParentUIDsByOrgUID(string orgUID)
    {
        ApiResponse<List<string>> apiResponse = await _apiService.FetchDataAsync<List<string>>(
            $"{_appConfigs.ApiBaseUrl}Org/GetOrgHierarchyParentUIDsByOrgUID",
            HttpMethod.Post, new List<string>
            {
                orgUID
            });
        return apiResponse.Data;
    }

    public async Task<List<IPurchaseOrderTemplateHeader>?> GetPurchaseOrderTemplateHeadersByStoreUidAndCreatedby(
        string storeUID, string? createdBy = null)
    {
        ApiResponse<List<IPurchaseOrderTemplateHeader>> apiResponse = await _apiService
            .FetchDataAsync<List<IPurchaseOrderTemplateHeader>>(
                $"{_appConfigs.ApiBaseUrl}PurchaseOrderTemplateHeader/GetPurchaseOrderTemplateHeadersByStoreUidAndCreatedby?storeUid={storeUID}&createdBy={createdBy}",
                HttpMethod.Get);

        return apiResponse != null && apiResponse.Data != null
            ? apiResponse.Data
            : (List<IPurchaseOrderTemplateHeader>?)default;
    }

    public async Task<List<IPurchaseOrderTemplateLine>?> GetAllPurchaseOrderTemplateLines(
        string purchaseOrderTemplateHeaderUID)
    {
        try
        {
            PagingRequest pagingRequest = new()
            {
                FilterCriterias =
                [
                    new FilterCriteria("purchaseordertemplateheaderuid", purchaseOrderTemplateHeaderUID,
                        FilterType.Equal)
                ]
            };
            ApiResponse<PagedResponse<IPurchaseOrderTemplateLine>> apiResponse =
                await _apiService.FetchDataAsync<PagedResponse<IPurchaseOrderTemplateLine>>(
                    $"{_appConfigs.ApiBaseUrl}PurchaseOrderTemplateLine/GetAllPurchaseOrderTemplateLines",
                    HttpMethod.Post, pagingRequest);

            if (apiResponse != null && apiResponse.IsSuccess && apiResponse.Data != null &&
                apiResponse.Data.PagedData != null)
            {
                return apiResponse.Data.PagedData.ToList();
            }
        }
        catch (Exception)
        {
            throw;
        }

        return default;
    }

    public async Task<List<ISellInSchemePO>?> GetSellInSchemesByOrgUidAndSKUUid(string OrgUid,
        List<string>? skus = null)
    {
        try
        {
            ApiResponse<List<SellInSchemePO>> apiResponse =
                await _apiService.FetchDataAsync<List<SellInSchemePO>>(
                    $"{_appConfigs.ApiBaseUrl}SellInSchemeHeader/GetSellInSchemesByOrgUidAndSKUUid?orgUID={OrgUid}",
                    HttpMethod.Post, skus);

            if (apiResponse != null && apiResponse.IsSuccess && apiResponse.Data != null && apiResponse.Data != null)
            {
                return apiResponse.Data.Cast<ISellInSchemePO>().ToList();
            }
        }
        catch (Exception)
        {
            throw;
        }

        return null;
    }
    public async Task<List<ISellInSchemePO>?> GetSellInSchemesByPOUid(string POUid)
    {
        try
        {
            ApiResponse<List<SellInSchemePO>> apiResponse =
                await _apiService.FetchDataAsync<List<SellInSchemePO>>(
                    $"{_appConfigs.ApiBaseUrl}SellInSchemeHeader/GetExistSellInSchemesByPOUid?POHeaderUID={POUid}",
                    HttpMethod.Get);

            if (apiResponse != null && apiResponse.IsSuccess && apiResponse.Data != null && apiResponse.Data != null)
            {
                return apiResponse.Data.Cast<ISellInSchemePO>().ToList();
            }
        }
        catch (Exception)
        {
            throw;
        }

        return null;
    }
    public async Task<List<ISellInSchemePO>?> GetExistSellInSchemesByPOUid(string POHeaderUID)
    {
        try
        {
            ApiResponse<List<SellInSchemePO>> apiResponse =
                await _apiService.FetchDataAsync<List<SellInSchemePO>>(
                    $"{_appConfigs.ApiBaseUrl}SellInSchemeHeader/GetExistSellInSchemesByPOUid?POHeaderUID={POHeaderUID}",
                    HttpMethod.Get);

            if (apiResponse != null && apiResponse.IsSuccess && apiResponse.Data != null && apiResponse.Data != null)
            {
                return apiResponse.Data.Cast<ISellInSchemePO>().ToList();
            }
        }
        catch (Exception)
        {
            throw;
        }

        return null;
    }

    public async Task<List<IQPSSchemePO>?> GetQPSSchemesByStoreUIDAndSKUUID(string storeUid, DateTime orderDate,
        List<ISKUFilter>? skus = null)
    {
        try
        {
            ApiResponse<List<QPSSchemePO>> apiResponse =
                await _apiService.FetchDataAsync<List<QPSSchemePO>>(
                    $"{_appConfigs.ApiBaseUrl}QPSSchemeHeader/GetQPSSchemesByStoreUIDAndSKUUID?storeUID={storeUid}&order_date={orderDate.ToString("yyyy-MM-dd")}",
                    HttpMethod.Post, skus);

            if (apiResponse != null && apiResponse.IsSuccess && apiResponse.Data != null && apiResponse.Data != null)
            {
                return apiResponse.Data.Cast<IQPSSchemePO>().ToList();
            }
        }
        catch (Exception)
        {
            throw;
        }

        return null;
    }
    public async Task<List<IQPSSchemePO>?> GetQPSSchemesByPOUid(string pouid,
        List<ISKUFilter>? skus = null)
    {
        try
        {
            ApiResponse<List<QPSSchemePO>> apiResponse =
                await _apiService.FetchDataAsync<List<QPSSchemePO>>(
                    $"{_appConfigs.ApiBaseUrl}QPSSchemeHeader/GetQPSSchemesByPOUID?pouid={pouid}",
                    HttpMethod.Post, skus);

            if (apiResponse != null && apiResponse.IsSuccess && apiResponse.Data != null && apiResponse.Data != null)
            {
                return apiResponse.Data.Cast<IQPSSchemePO>().ToList();
            }
        }
        catch (Exception)
        {
            throw;
        }

        return null;
    }

    public async Task<bool> SaveApprovalRequestDetails(string RequestId, string PurchaseOrderUID)
    {
        try
        {
            ApiResponse<string>? apiResponse = null;
            var allApprovalRequest = new AllApprovalRequest
            {
                LinkedItemType = "PurchaseOrder",
                LinkedItemUID = PurchaseOrderUID,
                RequestID = RequestId,
                ApprovalUserDetail = JsonConvert.SerializeObject(UserRole_Code)
            };

            apiResponse = await _apiService.FetchDataAsync(
                $"{_appConfigs.ApiBaseUrl}Store/CreateAllApprovalRequest", HttpMethod.Post, allApprovalRequest);
            // }

            if (apiResponse != null)
            {
                return apiResponse.IsSuccess;
            }
        }
        catch (Exception)
        {
            throw;
        }

        return false;
    }

    public async Task<List<IAllApprovalRequest>> GetAllApproveListDetailsFromAPIAsync(string UID)
    {
        try
        {
            Winit.Shared.Models.Common.ApiResponse<List<AllApprovalRequest>> AllApprovalLevelDetails = await
                _apiService.FetchDataAsync<List<AllApprovalRequest>>
                ($"{_appConfigs.ApiBaseUrl}Store/GetApprovalDetailsByStoreUID?LinkItemUID=" + UID, HttpMethod.Get,
                    null);
            if (AllApprovalLevelDetails != null && AllApprovalLevelDetails.IsSuccess &&
                AllApprovalLevelDetails.Data is not null)
            {
                return AllApprovalLevelDetails.Data.ToList<IAllApprovalRequest>();
            }

            return default;
        }
        catch (Exception)
        {
            throw;
        }
    }

    public async Task<bool> InsertDataInIntegrationDB(IPendingDataRequest pendingDataRequest)
    {
        try
        {
            Winit.Shared.Models.Common.ApiResponse<string> apiResponse = await
                _apiService.FetchDataAsync<string>
                ($"{_appConfigs.ApiBaseUrl}IntPendingDataInsertion/InsertPendingData", HttpMethod.Post,
                    pendingDataRequest);
            return apiResponse != null && apiResponse.IsSuccess;
        }
        catch (Exception)
        {
            throw;
        }
    }

    public async Task<List<IUserHierarchy>?> GetAllUserHierarchyFromAPIAsync(string hierarchyType, string hierarchyUID,
        int ruleId)
    {
        try
        {
            Winit.Shared.Models.Common.ApiResponse<List<UserHierarchy>> apiResponse = await
                _apiService.FetchDataAsync<List<UserHierarchy>>
                ($"{_appConfigs.ApiBaseUrl}MaintainUser/GetUserHierarchyForRule/{hierarchyType}/{hierarchyUID}/{ruleId}",
                    HttpMethod.Get);
            if (apiResponse != null && apiResponse.IsSuccess && apiResponse.Data is not null)
            {
                UserRole_Code = new Dictionary<string, List<string>>();

                foreach (var item in apiResponse.Data)
                {
                    string roleCode = item.RoleCode;
                    string userCode = item.EmpCode;
                    if (UserRole_Code.ContainsKey(roleCode))
                    {
                        UserRole_Code[roleCode].Add(userCode);
                    }
                    else
                    {
                        UserRole_Code[roleCode] = new List<string>
                        {
                            userCode
                        };
                    }
                }

                return apiResponse.Data.ToList<IUserHierarchy>();
            }

            return default;
        }
        catch (Exception)
        {
            throw;
        }
    }

    public async Task<IOrg?> GetOrgByUID(string orgUID)
    {
        try
        {
            Winit.Shared.Models.Common.ApiResponse<IOrg> apiResponse = await
                _apiService.FetchDataAsync<IOrg>
                    ($"{_appConfigs.ApiBaseUrl}Org/GetOrgByUID?UID={orgUID}", HttpMethod.Get);
            if (apiResponse != null && apiResponse.IsSuccess && apiResponse.Data is not null)
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

    public async Task<List<IOrg>?> GetOrgByOrgTypeUID(string orgTypeUID)
    {
        try
        {
            Winit.Shared.Models.Common.ApiResponse<List<IOrg>> apiResponse = await
                _apiService.FetchDataAsync<List<IOrg>>
                    ($"{_appConfigs.ApiBaseUrl}Org/GetOrgByOrgTypeUID?OrgTypeUID={orgTypeUID}", HttpMethod.Get);
            if (apiResponse != null && apiResponse.IsSuccess && apiResponse.Data is not null)
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

    public async Task<List<IAsmDivisionMapping>?> GetAsmDivisionMappingByUID(string linkedItemType,
        string linkedItemUID, string? asmEmpUID)
    {
        try
        {
            Winit.Shared.Models.Common.ApiResponse<List<AsmDivisionMapping>> apiResponse;
            if (!string.IsNullOrEmpty(asmEmpUID))
            {
                apiResponse = await
                    _apiService.FetchDataAsync<List<AsmDivisionMapping>>
                    ($"{_appConfigs.ApiBaseUrl}Store/GetAsmDivisionMappingByUIDV2/{linkedItemType}/{linkedItemUID}/{asmEmpUID}",
                        HttpMethod.Get);
            }
            else
            {
                apiResponse = await
                    _apiService.FetchDataAsync<List<AsmDivisionMapping>>
                    ($"{_appConfigs.ApiBaseUrl}Store/GetAsmDivisionMappingByUID/{linkedItemType}/{linkedItemUID}",
                        HttpMethod.Get);
            }

            if (apiResponse != null && apiResponse.IsSuccess && apiResponse.Data is not null)
            {
                return apiResponse.Data.Cast<IAsmDivisionMapping>().ToList();
            }

            return default;
        }
        catch (Exception ex)
        {
            throw;
        }
    }

    public async Task<string?> GetWareHouseUIDbySalesOfficeUID(string salesOfficeUID)
    {
        try
        {
            Winit.Shared.Models.Common.ApiResponse<string> apiResponse = await
                _apiService.FetchDataAsync<string>
                ($"{_appConfigs.ApiBaseUrl}SalesOffice/GetWareHouseUIDbySalesOfficeUID/{salesOfficeUID}",
                    HttpMethod.Get);
            if (apiResponse != null && apiResponse.IsSuccess && apiResponse.Data is not null)
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

    public async Task<IStoreCreditLimit?> GetCurrentLimitByStoreAndDivision(string storeUID, string divisionUID)
    {
        try
        {
            StoreCreditLimitRequest storeCreditLimitRequest = new StoreCreditLimitRequest
            {
                StoreUids = [storeUID],
                DivisionUID = divisionUID
            };

            Winit.Shared.Models.Common.ApiResponse<List<IStoreCreditLimit>> apiResponse = await
                _apiService.FetchDataAsync<List<IStoreCreditLimit>>
                ($"{_appConfigs.ApiBaseUrl}StoreCredit/GetCurrentLimitByStoreAndDivision", HttpMethod.Post,
                    storeCreditLimitRequest);
            if (apiResponse != null && apiResponse.IsSuccess && apiResponse.Data is not null && apiResponse.Data.Any())
            {
                return apiResponse.Data.First();
            }

            return default;
        }
        catch (Exception)
        {
            throw;
        }
    }

    public async Task<bool> CreateApproval(string purchaseOrderUid, ApprovalRequestItem? approvalRequestItem)
    {
        try
        {
            Winit.Shared.Models.Common.ApiResponse<string> apiResponse = await
                _apiService.FetchDataAsync<string>
                ($"{_appConfigs.ApiBaseUrl}PurchaseOrder/CreateApproval?purchaseOrderUid={purchaseOrderUid}",
                    HttpMethod.Post, approvalRequestItem);
            return apiResponse != null && apiResponse.IsSuccess;
        }
        catch (Exception)
        {
            throw;
        }
    }

    public async Task<List<IPurchaseOrderCreditLimitBufferRange>?> GetPurchaseOrderCreditLimitBufferRanges()
    {
        try
        {
            Winit.Shared.Models.Common.ApiResponse<List<IPurchaseOrderCreditLimitBufferRange>> apiResponse = await
                _apiService.FetchDataAsync<List<IPurchaseOrderCreditLimitBufferRange>>
                    ($"{_appConfigs.ApiBaseUrl}StoreCredit/GetPurchaseOrderCreditLimitBufferRanges", HttpMethod.Get);
            if (apiResponse != null && apiResponse.IsSuccess && apiResponse.Data is not null)
            {
                return apiResponse.Data;
            }

            return default;
        }
        catch (Exception e)
        {
            throw;
        }
    }

    public async Task<Dictionary<string, StandingSchemeResponse>?> GetStandingSchemesByOrgUidAndSKUUid(string orgUID,
        DateTime orderDate, List<ISKUFilter> skuFilterList)
    {
        try
        {
            Winit.Shared.Models.Common.ApiResponse<Dictionary<string, StandingSchemeResponse>> apiResponse = await
                _apiService.FetchDataAsync<Dictionary<string, StandingSchemeResponse>>
                ($"{_appConfigs.ApiBaseUrl}StandingProvisionScheme/GetStandingSchemesByOrgUidAndSKUUid?orgUID={orgUID}&orderDate={orderDate.ToString("yyyy-MM-dd")}",
                    HttpMethod.Post, skuFilterList);
            if (apiResponse != null && apiResponse.IsSuccess && apiResponse.Data is not null)
            {
                return apiResponse.Data;
            }

            return default;
        }
        catch (Exception e)
        {
            throw;
        }
    }
    public async Task<Dictionary<string, StandingSchemeResponse>?> GetStandingSchemesByPOUid(string pouid, List<ISKUFilter> skuFilterList)
    {
        try
        {
            Winit.Shared.Models.Common.ApiResponse<Dictionary<string, StandingSchemeResponse>> apiResponse = await
                _apiService.FetchDataAsync<Dictionary<string, StandingSchemeResponse>>
                ($"{_appConfigs.ApiBaseUrl}StandingProvisionScheme/GetStandingSchemesByPOUid?POUid={pouid}",
                    HttpMethod.Post, skuFilterList);
            if (apiResponse != null && apiResponse.IsSuccess && apiResponse.Data is not null)
            {
                return apiResponse.Data;
            }

            return default;
        }
        catch (Exception e)
        {
            throw;
        }
    }

    public async Task<bool> InsertEmailIntoRabbitMQ(List<string> smsTemplates, List<string> OrderUIDs)
    {
        try
        {
            List<NotificationRequest> notificationRequests = new List<NotificationRequest>();

            notificationRequests.AddRange(
                smsTemplates
                    .SelectMany(templateName => OrderUIDs
                        .Select(orderUID => new NotificationRequest
                        {
                            UniqueUID = Guid.NewGuid().ToString(),
                            LinkedItemType = "PurchaseOrder",
                            LinkedItemUID = orderUID,
                            TemplateName = templateName.ToString(),
                            NotificationRoute = NotificationRoutes.Notification_Order_Email
                        })));

            ApiResponse<string> apiResponse = await _apiService.FetchDataAsync(
                $"{_appConfigs.NotificationApiUrl}Notification/PublishMessagesByRoutingKey",
                HttpMethod.Post, notificationRequests);
            return apiResponse.IsSuccess;
        }
        catch (Exception ex)
        {
            throw;
        }
    }

    public async Task<bool> InsertSmsIntoRabbitMQ(List<string> smsTemplates, List<string> OrderUIDs)
    {
        try
        {
            List<NotificationRequest> notificationRequests = new List<NotificationRequest>();

            notificationRequests.AddRange(
                smsTemplates
                    .SelectMany(templateName => OrderUIDs
                        .Select(orderUID => new NotificationRequest
                        {
                            UniqueUID = Guid.NewGuid().ToString(),
                            LinkedItemType = "PurchaseOrder",
                            LinkedItemUID = orderUID,
                            TemplateName = templateName.ToString(),
                            NotificationRoute = NotificationRoutes.Notification_Order_SMS
                        })));

            ApiResponse<string> apiResponse = await _apiService.FetchDataAsync(
                $"{_appConfigs.NotificationApiUrl}Notification/PublishMessagesByRoutingKey",
                HttpMethod.Post, notificationRequests);
            return apiResponse.IsSuccess;
        }
        catch (Exception ex)
        {
            return false;
        }
    }

    public async Task<int> CheckSchemeExcludeMappingExists(string storeUID, DateTime currentDate)
    {
        SchemeExcludeMappingRequest schemeExcludeMappingRequest =
            new() { StoreUID = storeUID, CurrentDate = currentDate };
        ApiResponse<int> apiResponse = await _apiService.FetchDataAsync<int>(
            $"{_appConfigs.ApiBaseUrl}SchemeExcludeController/CheckSchemeExcludeMappingExists?storeUID={storeUID}&currentDate={currentDate.ToString()}",
            HttpMethod.Post, schemeExcludeMappingRequest);
        return apiResponse != null && apiResponse.IsSuccess ? apiResponse.Data : default;
    }
}