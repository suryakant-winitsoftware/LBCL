
using Winit.Modules.Base.BL;
using Winit.Modules.Common.BL.Interfaces;
using Winit.Modules.Common.Model.Classes;
using Winit.Modules.Common.Model.Interfaces;
using Winit.Modules.SKU.BL.Interfaces;
using Winit.Shared.CommonUtilities.Common;
using Winit.Shared.CommonUtilities.Extensions;
using Winit.Shared.Models.Common;
using Winit.Shared.Models.Constants;

namespace Winit.Modules.SKU.BL.Classes
{
    public class SKUPriceViewModel : ISKUPriceViewModel
    {
        IServiceProvider _serviceProvider { get; }
        IAppConfig _appConfig { get; }
        ApiService _apiService { get; }
        CommonFunctions _commonFunctions { get; }
        IAppUser _appUser { get; }
        public PagingRequest PagingRequest { get; set; }
        public SKUPriceViewModel(IServiceProvider serviceProvider, IAppConfig appConfig, ApiService apiService,
            CommonFunctions commonFunctions, IAppUser appUser)
        {
            _serviceProvider = serviceProvider;
            _appConfig = appConfig;
            _apiService = apiService;
            _commonFunctions = commonFunctions;
            _appUser = appUser;
            SKUPriceView = _serviceProvider.CreateInstance<Winit.Modules.SKU.Model.Interfaces.ISKUPriceView>();
            PopulateViewModel();
        }
        public bool IsIndividualPricelist { get; set; }
        public int TotalItems { get; set; }
       
        Winit.Shared.Models.Common.PagingRequest _pagingRequest = new()
        {
            FilterCriterias = [],
            SortCriterias = []
        };
        public Winit.Modules.SKU.Model.Interfaces.ISKUPriceView SKUPriceView { get; }

        public bool IsNew { get; set; }
        public async Task PopulateViewmodel(string priceListUID)
        {
            if (!IsNew)
            {
                SKUPriceView.SKUPriceGroup.UID = priceListUID;
                await GetDataFromAPIAsync();
            }
        }
        private void PopulateViewModel()
        {
            IsNew = PageType.New == _commonFunctions.GetParameterValueFromURL(PageType.Page);
            SKUPriceView.SKUPriceGroup = _serviceProvider.CreateInstance<Winit.Modules.SKU.Model.Interfaces.ISKUPriceList>();
            SKUPriceView.SKUPriceList = [];
            SKUPriceView.SKUPriceGroup.ValidFrom = DateTime.Now;
            SKUPriceView.SKUPriceGroup.ValidUpto = new DateTime(2099, 12, 31);
            if (IsNew)
            {
                SKUPriceView.SKUPriceGroup.UID = Guid.NewGuid().ToString();
            }
        }

        private void SaveOrUpdate()
        {
            if (IsNew)
            {
                SKUPriceView.SKUPriceGroup.CreatedBy = _appUser.Emp.UID;
                SKUPriceView.SKUPriceGroup.ModifiedBy = _appUser.Emp.UID;
                SKUPriceView.SKUPriceGroup.CreatedTime = DateTime.Now;
                SKUPriceView.SKUPriceGroup.ModifiedTime = DateTime.Now;
                SKUPriceView.SKUPriceGroup.OrgUID = _appUser.SelectedJobPosition.OrgUID;
                SKUPriceView.SKUPriceGroup.Type = IsIndividualPricelist ? "FRC" : "FR";
            }
            else
            {
                SKUPriceView.SKUPriceGroup.ModifiedBy = _appUser.Emp.UID;
                SKUPriceView.SKUPriceGroup.ModifiedTime = DateTime.Now;
            }
            DatamanagerGeneric<Winit.Modules.SKU.Model.Interfaces.ISKUPriceView>.Set(nameof(SKUPriceView), SKUPriceView);
        }



        #region api calling
        private async Task GetDataFromAPIAsync()
        {
            try
            {
                Winit.Shared.Models.Common.ApiResponse<PagedResponse<Winit.Modules.SKU.Model.Interfaces.ISKUPriceView>> apiResponse =
                    await _apiService.FetchDataAsync<PagedResponse<Winit.Modules.SKU.Model.Interfaces.ISKUPriceView>>(
                    $"{_appConfig.ApiBaseUrl}SKUPrice/SelectSKUPriceViewByUID?UID={SKUPriceView.SKUPriceGroup.UID}",
                    HttpMethod.Post, _pagingRequest);

                if (apiResponse != null && apiResponse.IsSuccess)
                {

                    if (apiResponse.Data != null && apiResponse.Data.PagedData is not null && apiResponse.Data.PagedData.Any())
                    {
                        Winit.Modules.SKU.Model.Interfaces.ISKUPriceView sKUPriceView = apiResponse.Data.PagedData!.FirstOrDefault()!;
                        if (sKUPriceView != null)
                        {
                            if (sKUPriceView.SKUPriceGroup == null)
                                IsNew = true;
                            else
                                SKUPriceView.SKUPriceGroup = sKUPriceView.SKUPriceGroup;
                            if (sKUPriceView.SKUPriceList != null)
                            {
                                SKUPriceView.SKUPriceList.Clear();
                                SKUPriceView.SKUPriceList.AddRange(sKUPriceView.SKUPriceList);
                                TotalItems = apiResponse.Data.TotalCount;
                            }
                        }
                    }
                    else
                    {
                        TotalItems = 0;
                    }
                }
            }
            catch (Exception ex)
            {
                TotalItems = 0;
            }
        }
        public async Task<ApiResponse<string>> SaveSKUPrices(List<Winit.Modules.SKU.Model.Interfaces.ISKUPrice> sKUPrices)
        {
            SaveOrUpdate();
            SKUPriceView.SKUPriceList.Clear();
            SKUPriceView.SKUPriceList.AddRange(sKUPrices);
            string response = string.Empty;
            string responseMethod = IsNew ? "CreateSKUPriceView" : "UpdateSKUPriceView";
            if (!IsNew && IsIndividualPricelist)
            {
                responseMethod = "UpdateSKUPriceList";
            }
            Winit.Shared.Models.Common.ApiResponse<string> apiResponse = await _apiService.FetchDataAsync(
                  $"{_appConfig.ApiBaseUrl}SKUPrice/{responseMethod}",
                  IsNew ? HttpMethod.Post : HttpMethod.Put, !IsNew && IsIndividualPricelist ? SKUPriceView.SKUPriceList : SKUPriceView);

            return apiResponse;
        }
        #endregion
    }
}
