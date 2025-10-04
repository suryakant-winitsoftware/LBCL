using Winit.Modules.Base.BL;
using Winit.Modules.CollectionModule.Model.Classes;
using Winit.Modules.CollectionModule.Model.Interfaces;
using Winit.Modules.Common.BL.Interfaces;
using Winit.Modules.Notification.Model.Classes;
using Winit.Modules.Notification.Model.Constant;
using Winit.Shared.Models.Common;

namespace Winit.Modules.CollectionModule.BL.Classes.BalanceConfirmation
{
    public class BalanceConfirmationWebViewModel : BalanceConfirmationBaseViewModel
    {
        protected readonly ApiService _apiService;
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 10;
        public int TotalCount { get; set; } = 0;
        public BalanceConfirmationWebViewModel(IServiceProvider serviceProvider, IAppConfig appConfig, IAppUser appUser, ApiService apiService) : base(serviceProvider, appConfig, appUser)
        {
            _apiService = apiService;
        }

        public override async Task<List<IStoreStatement>> GetStoreStatementData()
        {
            try
            {
                PagingRequest pagingRequest = new PagingRequest();
                pagingRequest.PageNumber = PageNumber;
                pagingRequest.PageSize = PageSize;
                pagingRequest.SortCriterias = SortCriterias;
                pagingRequest.FilterCriterias = FilterCriterias;
                pagingRequest.IsCountRequired = true;
                Winit.Shared.Models.Common.ApiResponse<PagedResponse<StoreStatement>> StoreStatement = await _apiService.FetchDataAsync<PagedResponse<StoreStatement>>
                ($"{_appConfig.ApiBaseUrl}CollectionModule/StoreStatementRecords?StartDate=" + BalanceConfirmationDetails?.StartDate.ToString("yyyy-MM-dd") + "&EndDate=" + BalanceConfirmationDetails?.EndDate.ToString("yyyy-MM-dd"), HttpMethod.Post, pagingRequest);
                if (StoreStatement != null && StoreStatement.IsSuccess)
                {
                    TotalCount = StoreStatement.Data.TotalCount;
                    return StoreStatementDetails = StoreStatement.Data.PagedData.ToList<IStoreStatement>();
                }


                return default;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message + " viewpayments.razor exception");
            }
        }

        public override async Task GetBalanceConfirmationTableDetails(string StoreUID)
        {
            try
            {
                Winit.Shared.Models.Common.ApiResponse<Winit.Modules.CollectionModule.Model.Classes.BalanceConfirmation> BalanceConfirmation = await _apiService.FetchDataAsync<Winit.Modules.CollectionModule.Model.Classes.BalanceConfirmation>
                ($"{_appConfig.ApiBaseUrl}CollectionModule/GetBalanceConfirmationDetails?StoreUID=" + StoreUID, HttpMethod.Get);
                if (BalanceConfirmation != null && BalanceConfirmation.IsSuccess)
                {
                    BalanceConfirmationDetails = BalanceConfirmation.Data;
                }
            }
            catch (Exception ex)
            {

            }
        }
        public override async Task GetContactDetails(string EmpCode)
        {
            try
            {
                Winit.Shared.Models.Common.ApiResponse<Winit.Modules.Contact.Model.Classes.Contact> contactDetails = await _apiService.FetchDataAsync<Winit.Modules.Contact.Model.Classes.Contact>
                ($"{_appConfig.ApiBaseUrl}CollectionModule/GetContactDetails?EmpCode=" + EmpCode, HttpMethod.Get);
                if (contactDetails != null && contactDetails.IsSuccess)
                {
                    ContactDetails = contactDetails.Data;
                }
            }
            catch (Exception ex)
            {

            }
        }
        public override async Task GetBalanceConfirmationTableListDetails()
        {
            try
            {
                ApiResponse<List<Model.Classes.BalanceConfirmation>> BalanceConfirmation = await _apiService.FetchDataAsync<List<Model.Classes.BalanceConfirmation>>
                ($"{_appConfig.ApiBaseUrl}CollectionModule/GetBalanceConfirmationListDetails", HttpMethod.Get);
                if (BalanceConfirmation != null && BalanceConfirmation.IsSuccess)
                {
                    BalanceConfirmationListDetails = BalanceConfirmation.Data.ToList<IBalanceConfirmation>();
                }
            }
            catch (Exception ex)
            {

            }
        }
        public override async Task GetBalanceConfirmationLineTableDetails(string UID)
        {
            try
            {
                Winit.Shared.Models.Common.ApiResponse<List<Winit.Modules.CollectionModule.Model.Classes.BalanceConfirmationLine>> BalanceConfirmationLine = await _apiService.FetchDataAsync<List<Winit.Modules.CollectionModule.Model.Classes.BalanceConfirmationLine>>
                ($"{_appConfig.ApiBaseUrl}CollectionModule/GetBalanceConfirmationLineDetails?UID=" + UID, HttpMethod.Get);
                if (BalanceConfirmationLine != null && BalanceConfirmationLine.IsSuccess)
                {
                    BalanceConfirmationLineDetails = BalanceConfirmationLine.Data.ToList<IBalanceConfirmationLine>();
                }
            }
            catch (Exception ex)
            {

            }
        }

        public override async Task<bool> InsertDisputeRecords(List<IBalanceConfirmationLine> balanceConfirmationLine)
        {
            try
            {
                ApiResponse<string> apiResponse = await _apiService.FetchDataAsync
                ($"{_appConfig.ApiBaseUrl}CollectionModule/InsertDisputeRecords", HttpMethod.Post, balanceConfirmationLine);
                if (apiResponse != null && apiResponse.IsSuccess && apiResponse.Data != null)
                {
                    return Convert.ToBoolean(apiResponse.Data);
                }
                else
                {
                    return false;
                }
            }
            catch (Exception ex)
            {
                return false;
            }
        }
        public override async Task<bool> UpdateDisputeResolved(IBalanceConfirmation balanceConfirmation)
        {
            try
            {
                ApiResponse<bool> apiResponse = await _apiService.FetchDataAsync<bool>
                ($"{_appConfig.ApiBaseUrl}CollectionModule/UpdateBalanceConfirmationForResolvingDispute", HttpMethod.Put, balanceConfirmation);
                if (apiResponse != null && apiResponse.IsSuccess && apiResponse.Data != null)
                {
                    return apiResponse.Data;
                }
                else
                {
                    return false;
                }
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        public override async Task<bool> UpdateBalanceConfirmation(IBalanceConfirmation balanceConfirmation)
        {
            try
            {
                ApiResponse<bool> apiResponse = await _apiService.FetchDataAsync<bool>
                ($"{_appConfig.ApiBaseUrl}CollectionModule/UpdateBalanceConfirmation", HttpMethod.Put, balanceConfirmation);
                if (apiResponse != null && apiResponse.IsSuccess && apiResponse.Data != null)
                {
                    return apiResponse.Data;
                }
                else
                {
                    return false;
                }
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        public override async Task<bool> InsertSmsIntoRabbitMQ(string Otp, string MobileNumber)
        {
            try
            {
                List<NotificationRequest> notificationRequests = new List<NotificationRequest>();
                notificationRequests.Add(new NotificationRequest
                {
                    UniqueUID = Guid.NewGuid().ToString(),
                    LinkedItemType = "SelfRegistration",
                    LinkedItemUID = Otp,
                    TemplateName = "General",
                    Receiver = new List<string> { MobileNumber },
                    NotificationRoute = NotificationRoutes.Notification_General_SMS
                });

                ApiResponse<string> apiResponse = await _apiService.FetchDataAsync(
                   $"{_appConfig.NotificationApiUrl}Notification/PublishMessagesByRoutingKey",
                   HttpMethod.Post, notificationRequests);
                return apiResponse.IsSuccess;
            }
            catch (Exception ex)
            {
                return false;
            }
        }
    }
}
