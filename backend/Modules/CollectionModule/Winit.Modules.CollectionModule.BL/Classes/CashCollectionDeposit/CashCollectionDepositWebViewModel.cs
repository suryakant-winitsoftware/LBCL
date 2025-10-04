using Azure;
using Dapper;

using Nest;
using Newtonsoft.Json;
using System.Linq;
using Winit.Modules.Base.BL;
using Winit.Modules.CollectionModule.Model.Classes;
using Winit.Modules.CollectionModule.Model.Interfaces;
using Winit.Modules.Common.BL.Interfaces;
using Winit.Modules.FileSys.Model.Classes;
using Winit.Modules.FileSys.Model.Interfaces;
using Winit.Shared.CommonUtilities.Common;
using Winit.Shared.Models.Common;
using Winit.Shared.Models.Enums;
using static Winit.Modules.CollectionModule.Model.Classes.AccCollectionDeposit;

namespace Winit.Modules.CollectionModule.BL.Classes.CashCollectionDeposit
{
    public class CashCollectionDepositWebViewModel : CashCollectionDepositBaseViewModel
    {
        protected readonly ApiService _apiService;
        public CashCollectionDepositWebViewModel(IServiceProvider serviceProvider, IAppConfig appConfig, IAppUser appUser, ApiService apiService) : base(serviceProvider, appConfig, appUser)
        {
            _apiService = apiService;
        }

        public override Task<bool> CreateCashDepositRequest()
        {
            throw new NotImplementedException();
        }

        public override Task<List<IAccCollection>> GetReceipts()
        {
            throw new NotImplementedException();
        }

        public override async Task<List<IAccCollectionDeposit>> GetRequestReceipts(string Status)
        {
            try
            {
                Winit.Shared.Models.Common.ApiResponse<List<AccCollectionDeposit>> apiResponse =
                         await _apiService.FetchDataAsync<List<AccCollectionDeposit>>($"{_appConfig.ApiBaseUrl}CollectionModule/GetRequestReceipts?Status=" + Status
                         , HttpMethod.Get);

                if (apiResponse != null && apiResponse.Data != null)
                {
                    return apiResponse.Data.ToList<IAccCollectionDeposit>();
                }
                return default;
            }
            catch(Exception ex)
            {
                throw new();
            }
        }

        public override async Task<List<IAccCollection>> ViewReceipts(string RequestNo)
        {
            try
            {
                Winit.Shared.Models.Common.ApiResponse<CollectionDepositDTO> apiResponse =
                         await _apiService.FetchDataAsync<CollectionDepositDTO>($"{_appConfig.ApiBaseUrl}CollectionModule/ViewReceipts?RequestNo=" + RequestNo
                         , HttpMethod.Get);
                CollectionDepositDTO accCollectionAndDeposits = new CollectionDepositDTO();

                if (apiResponse != null && apiResponse.Data != null)
                {
                    accCollectionAndDeposits = apiResponse.Data;
                    CashCollectionDeposit = accCollectionAndDeposits.AccCollectionDeposits.First();
                    AccCollections= accCollectionAndDeposits.AccCollections.ToList<IAccCollection>();
                    await GetFileSys();
                    return AccCollections;
                }
                return default;
            }
            catch (Exception ex)
            {
                throw new();
            }
        }
        public Winit.Shared.Models.Common.ApiResponse<string> response { get; set; }
        public override async Task<string> ApproveOrRejectDepositRequest(IAccCollectionDeposit accCollectionDeposit, string Status)
        {
            try
            {
                accCollectionDeposit.CreatedBy = _appUser.Emp.UID;
                accCollectionDeposit.ModifiedBy = _appUser.Emp.UID;
                response = await _apiService.FetchDataAsync<string>($"{_appConfig.ApiBaseUrl}CollectionModule/ApproveOrRejectDepositRequest?Status={Status}", HttpMethod.Post, accCollectionDeposit);
                return response.Data;
            }
            catch (Exception ex)
            {
                throw new();
            }
        }

        public async Task GetFileSys()
        {
            FileSysList = new();
            PagingRequest paging = new PagingRequest();
            paging.FilterCriterias = new List<FilterCriteria>()
            {
                new FilterCriteria("LinkedItemUID",CashCollectionDeposit.RequestNo, FilterType.In)
            };
            paging.IsCountRequired = true;

            ApiResponse<string> apiResponse = await _apiService.FetchDataAsync(
            $"{_appConfig.ApiBaseUrl}FileSys/SelectAllFileSysDetails",
              HttpMethod.Post, paging);
            if (apiResponse.Data != null)
            {
                var data = new CommonFunctions().GetDataFromResponse(apiResponse.Data);
                PagedResponse<FileSys.Model.Classes.FileSys>? pagedResponse = JsonConvert.DeserializeObject<PagedResponse<FileSys.Model.Classes.FileSys>>(data);
                if (pagedResponse != null)
                {
                    try
                    {
                        if (pagedResponse.TotalCount > 0)
                        {
                            FileSysList = pagedResponse.PagedData.ToList<IFileSys>();
                        }
                    }
                    catch (Exception ex)
                    {

                    }
                }

            }

        }
    }
}
