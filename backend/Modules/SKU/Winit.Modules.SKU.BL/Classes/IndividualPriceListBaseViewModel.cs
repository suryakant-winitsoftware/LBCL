using iTextSharp.text;
using Microsoft.AspNetCore.Components;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Winit.Modules.Base.BL;
using Winit.Modules.SKU.BL.Interfaces;
using Winit.Modules.SKU.Model.Classes;
using Winit.Shared.CommonUtilities.Common;
using Winit.Shared.Models.Common;
using Winit.UIComponents.Common;

namespace Winit.Modules.SKU.BL.Classes
{
    public class IndividualPriceListBaseViewModel: IIndividualPriceListBaseViewModel
    {
        IAppConfig _appConfig;
        CommonFunctions _commonFunctions;
        IAlertService _alertService;
        ApiService _apiService; 
        public IndividualPriceListBaseViewModel(IAppConfig appConfig, CommonFunctions commonFunctions, IAlertService alertService, ApiService apiService)
        {
            _appConfig = appConfig;
            _commonFunctions = commonFunctions;
            _alertService = alertService;
            _apiService = apiService;
        }
        public SKUPriceViewDTO? sKUPriceViewDTO { get; set; }
        public int _currentPage { get; set; } = 1;

        public int _pageSize = 20;

        public int _totalItems = 0;
        private string priceListUID = string.Empty;
        public bool IsNewPriceList {  get; set; }
        public List<Winit.Modules.SKU.Model.Classes.SKUPrice> SKUPriceList { get; set; }
        public async Task GetDataFromAPIAsync()
        {
            PagingRequest pagingRequest = new PagingRequest();
            pagingRequest.SortCriterias = new List<Winit.Shared.Models.Enums.SortCriteria>();
            pagingRequest.PageNumber = _currentPage;
            pagingRequest.PageSize = _pageSize;
            pagingRequest.FilterCriterias = null;
            pagingRequest.IsCountRequired = true;
            try
            {
                Winit.Shared.Models.Common.ApiResponse<string> apiResponse = await _apiService.FetchDataAsync(
                    $"{_appConfig.ApiBaseUrl}SKUPrice/SelectSKUPriceViewByUID?UID={priceListUID}",
                HttpMethod.Post, pagingRequest);


                if (apiResponse != null && apiResponse.IsSuccess && apiResponse.Data != null)
                {
                    string data = _commonFunctions.GetDataFromResponse(apiResponse.Data);

                    Winit.Shared.Models.Common.PagedResponse<SKUPriceViewDTO> pagedResponse = JsonConvert.DeserializeObject<PagedResponse<SKUPriceViewDTO>>(data);
                    if (pagedResponse != null)
                    {
                        if (pagedResponse.PagedData != null)
                        {
                            sKUPriceViewDTO = pagedResponse?.PagedData?.FirstOrDefault();
                            if (sKUPriceViewDTO?.SKUPriceGroup == null)
                            {
                                IsNewPriceList = true;
                                sKUPriceViewDTO.SKUPriceGroup = new();
                            }
                            else
                            {
                                if (sKUPriceViewDTO.SKUPriceList != null)
                                {
                                    SKUPriceList = sKUPriceViewDTO.SKUPriceList;
                                }
                            }

                        }
                    }

                }
                else
                {
                    _totalItems = 0;
                }
            }
            catch (Exception ex)
            {
                _totalItems = 0;
            }
        }
        public async Task Save(List<SKUPrice> SKUPriceList)
        {
           
        }
    }
}
