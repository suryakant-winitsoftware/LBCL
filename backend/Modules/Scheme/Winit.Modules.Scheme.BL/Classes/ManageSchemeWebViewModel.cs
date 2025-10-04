using Microsoft.AspNetCore.Components;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Winit.Modules.Base.BL;
using Winit.Modules.Common.BL.Interfaces;
using Winit.Modules.Promotion.Model.Classes;
using Winit.Modules.Scheme.BL.Interfaces;
using Winit.Modules.Scheme.Model.Classes;
using Winit.Modules.Scheme.Model.Constants;
using Winit.Modules.Scheme.Model.Interfaces;
using Winit.Shared.CommonUtilities.Common;
using Winit.Shared.Models.Common;
using Winit.Shared.Models.Constants;
using Winit.Shared.Models.Enums;

namespace Winit.Modules.Scheme.BL.Classes
{
    public class ManageSchemeWebViewModel : ManageSchemeBaseViewModel
    {
        ApiService _apiService { get; }
        IAppConfig _appConfig { get; }
        ISchemeViewModelBase _schemeViewModelBase { get; }
        IAppUser _appUser { get; }

        public ManageSchemeWebViewModel(ApiService apiService, IAppUser appUser, IAppConfig appConfig,
            ISchemeViewModelBase schemeViewModelBase)
        {
            _apiService = apiService;
            _appConfig = appConfig;
            _schemeViewModelBase = schemeViewModelBase;
            _appUser = appUser;
            ManageSchemes = [];
        }

        public override async Task PopulateViewModel()
        {
            PagingRequest.PageSize = PageSize;
            PagingRequest.PageNumber = CurrentPage;
            List<string> status = [SchemeConstants.Approved, SchemeConstants.Pending];
            PagingRequest.FilterCriterias = [(new(name: nameof(IManageScheme.Status), status, FilterType.In))];
            List<Task> tasks = new List<Task>()
            {
               // GetAllSchemes(),
                _schemeViewModelBase.GetAllChannelPartner()
            };
            await Task.WhenAll(tasks);
            ChannelPartner = _schemeViewModelBase.ChannelPartner;
        }

        protected override async Task GetAllSchemes()
        {
            Winit.Shared.Models.Common.ApiResponse<PagedResponse<IManageScheme>> apiResponse =
                await _apiService.FetchDataAsync<PagedResponse<IManageScheme>>(
                    $"{_appConfig.ApiBaseUrl}ManageScheme/SelectAllSchemes/{_appUser.SelectedJobPosition.UID}/{_appUser.Role.IsAdmin}",
                    HttpMethod.Post, PagingRequest);


            if (apiResponse != null && apiResponse.IsSuccess && apiResponse.Data != null)
            {
                ManageSchemes = apiResponse!.Data!.PagedData!.ToList();
                TotalItems = apiResponse!.Data!.TotalCount;
            }
        }

        public async Task<List<Winit.Modules.ListHeader.Model.Interfaces.IListItem>> GetListItemsByCodes()
        {
            Winit.Modules.ListHeader.Model.Classes.ListItemRequest listItemRequest =
                new Winit.Modules.ListHeader.Model.Classes.ListItemRequest()
                {
                    isCountRequired = true,
                    Codes = new List<string>()
                    {
                        "SchemeTypes"
                    }
                };
            Shared.Models.Common.ApiResponse<PagedResponse<Winit.Modules.ListHeader.Model.Interfaces.IListItem>>
                apiResponse =
                    await _apiService
                        .FetchDataAsync<PagedResponse<Winit.Modules.ListHeader.Model.Interfaces.IListItem>>(
                            $"{_appConfig.ApiBaseUrl}ListItemHeader/GetListItemsByCodes", HttpMethod.Post,
                            listItemRequest);

            return apiResponse.IsSuccess && apiResponse.Data is not null && apiResponse.Data!.PagedData != null
                ? apiResponse.Data!.PagedData.ToList()
                : null;
        }

        public async Task<ApiResponse<string>> ChangeEndDate(PromotionView promotion)
        {
            string url = "Promotion/ChangeEndDate";

            if (promotion.SchemeExtendHistory.SchemeType.ToLower() == "qps" ||
                promotion.SchemeExtendHistory.SchemeType.Equals("SellOutRealSecondary",
                    StringComparison.OrdinalIgnoreCase))
            {
                url = "Promotion/ChangeEndDate";
            }

            promotion.EndDateUpdatedOn = DateTime.Now;
            promotion.ModifiedTime = DateTime.Now;
            promotion.EndDateUpdatedByEmpUID = _appUser.Emp.UID;
            promotion.ModifiedBy = _appUser.Emp.UID;

            return await ChangeEndDate(scheme: promotion, url);
        }

        protected async Task<ApiResponse<string>> ChangeEndDate(object scheme, string url)
        {
            Winit.Shared.Models.Common.ApiResponse<string> apiResponse = await _apiService.FetchDataAsync(
                $"{_appConfig.ApiBaseUrl}{url}",
                HttpMethod.Put, scheme);
            return apiResponse;
        }

        public override async Task GetschemeExtendHistoryBySchemeUID(string schemeUID)
        {
            Shared.Models.Common.ApiResponse<List<ISchemeExtendHistory>> apiResponse = await
                _apiService.FetchDataAsync<List<ISchemeExtendHistory>>
                ($"{_appConfig.ApiBaseUrl}ManageScheme/GetschemeExtendHistoryBySchemeUID/{schemeUID}",
                    HttpMethod.Get);
            if (apiResponse != null)
            {
                if (apiResponse.IsSuccess && apiResponse.StatusCode == 200)
                {
                    SchemeExtendHistories = apiResponse?.Data;
                }
            }
        }
    }
}