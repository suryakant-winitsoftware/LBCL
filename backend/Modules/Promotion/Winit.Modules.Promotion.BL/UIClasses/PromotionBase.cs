using Microsoft.AspNetCore.Components;
using Nest;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Winit.Modules.Base.BL;
using Winit.Modules.ListHeader.Model.Classes;
using Winit.Modules.Promotion.BL.UIInterfaces;
using Winit.Modules.Promotion.Model.Classes;
using Winit.Shared.CommonUtilities.Common;
using Winit.Shared.Models.Common;
using Winit.Shared.Models.Constants;
using Winit.Shared.Models.Enums;
using Winit.UIComponents.Common;
using Winit.UIModels.Common;

namespace Winit.Modules.Promotion.BL.UIClasses
{
    public class PromotionBase : UIInterfaces.IPromotionBase
    {
        public PromotionBase(ApiService apiService, IAppConfig appConfigs, CommonFunctions commonFunctions, NavigationManager navigationManager, Winit.Modules.Common.Model.Interfaces.IDataManager dataManager, IAlertService alertService)
        {
            this._apiService = apiService;
            this._appConfigs = appConfigs;
            this._commonFunctions = commonFunctions;
            this._navigationManager = navigationManager;
            this._dataManager = dataManager;
            this._alertService = alertService;
        }
        public PagingRequest PagingRequest { get; set; } = new()
        {
            PageNumber = 1,
            PageSize = 10,
            FilterCriterias = [],
            SortCriterias = [],
            IsCountRequired = true,
        };
        public int TotalItems { get; set; }
        public string PromotionType { get; set; }
        public bool IsLoad { get; set; }
        public bool IsAddEditLoad { get; set; }
        public ApiService _apiService { get; set; }
        public IAppConfig _appConfigs { get; set; }
        public CommonFunctions _commonFunctions { get; set; }
        public NavigationManager _navigationManager { get; set; }
        public Common.Model.Interfaces.IDataManager _dataManager { get; set; }
        public IAlertService _alertService { get; set; }

        /// <summary>
        /// Add Edit Promotion
        /// </summary>
        /// <returns></returns>

        string pagetype = string.Empty;
        public List<Winit.Modules.Promotion.Model.Classes.Promotion> PromotionsList { get; set; } = [];

        public List<ListItem> PromotionsDropDowns { get; set; }


        FilterCriteria DefaultFilter = new FilterCriteria("type", new List<string>()
            {
                 Winit.Shared.Models.Constants.Promotions.Line,
                 Winit.Shared.Models.Constants.Promotions.Assorted,
                 Winit.Shared.Models.Constants.Promotions.Invoice,
            }, FilterType.In);
        public async Task PageLoadFieldsOfMaintainPromotion()
        {


            //await GetPromotionDetails();


        }
        public async Task OnFilterApply(IDictionary<string, string> filterCriteria)
        {
            PagingRequest.FilterCriterias!.Clear();
            PagingRequest.SortCriterias!.Clear();
            foreach (var item in filterCriteria)
            {
                if (!string.IsNullOrEmpty(item.Value))
                {
                    FilterCriteria filter = new FilterCriteria(item.Key, item.Value, FilterType.Like);
                    switch (item.Key)
                    {
                        case nameof(Winit.Modules.Promotion.Model.Classes.Promotion.ValidFrom):
                            filter.Type = FilterType.GreaterThanOrEqual;
                            filter.Value = CommonFunctions.GetDate(item.Value);
                            break;
                        case nameof(Winit.Modules.Promotion.Model.Classes.Promotion.ValidUpto):
                            filter.Type = FilterType.LessThanOrEqual;
                            filter.Value = CommonFunctions.GetDate(item.Value);
                            break;
                        case nameof(Winit.Modules.Promotion.Model.Classes.Promotion.Type):
                            filter.Type = FilterType.Equal;
                            break;
                    }
                    PagingRequest.FilterCriterias.Add(filter);
                }
            }
            await GetPromotionDetails();
        }
        public async Task OnSort(SortCriteria sortCriteria)
        {
            PagingRequest.SortCriterias!.Clear();
            PagingRequest.SortCriterias.Add(sortCriteria);
            await GetPromotionDetails();
        }
        public async Task GetPromotionDetails()
        {
            PagingRequest.FilterCriterias!.Add(DefaultFilter);
            if (PagingRequest.SortCriterias != null && PagingRequest.SortCriterias.Count == 0)
            {
                PagingRequest.SortCriterias.Add(new SortCriteria(nameof(Winit.Modules.Promotion.Model.Classes.Promotion.ModifiedTime), SortDirection.Desc));
            }
            Shared.Models.Common.ApiResponse<PagedResponse<Winit.Modules.Promotion.Model.Classes.Promotion>> apiResponse =
                await _apiService.FetchDataAsync<PagedResponse<Winit.Modules.Promotion.Model.Classes.Promotion>>($"{_appConfigs.ApiBaseUrl}Promotion/GetPromotionDetails", HttpMethod.Post, PagingRequest);
            if (apiResponse != null)
            {
                if (apiResponse.IsSuccess && apiResponse.StatusCode == 200 && apiResponse.Data != null && apiResponse.Data.PagedData != null)
                {
                    PromotionsList.Clear();
                    PromotionsList.AddRange(apiResponse.Data.PagedData);
                    TotalItems = apiResponse.Data.TotalCount;
                }
            }
        }
    }
}

