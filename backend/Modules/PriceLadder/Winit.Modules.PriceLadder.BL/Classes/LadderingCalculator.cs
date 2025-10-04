using Nest;
using System.Collections.Generic;
using Winit.Modules.Base.BL;
using Winit.Modules.PriceLadder.BL.Interfaces;
using Winit.Modules.PriceLadder.Model.Interfaces;
using Winit.Shared.Models.Common;

namespace Winit.Modules.PriceLadder.BL.Classes;

public class LadderingCalculator : ILadderingCalculator
{
    private readonly IAppConfig _appConfigs;
    private readonly ApiService _apiService;
    private List<Winit.Modules.PriceLadder.Model.Interfaces.ISKUPriceLadderingData> SKUPriceLadderingDatas;
    private bool HasFullData = false;

    public LadderingCalculator(IAppConfig appConfigs,
            ApiService apiService)
    {
        this._appConfigs = appConfigs;
        this._apiService = apiService;
        SKUPriceLadderingDatas = [];
    }
    private async Task<List<Winit.Modules.PriceLadder.Model.Interfaces.ISKUPriceLadderingData>?> GetApplicablePriceLaddering(
        string broadCustomerClassification, DateTime date, List<int>? productCategoryIds = null)
    {
        ApiResponse<List<Winit.Modules.PriceLadder.Model.Interfaces.ISKUPriceLadderingData>> apiResponse = await _apiService
            .FetchDataAsync<List<Winit.Modules.PriceLadder.Model.Interfaces.ISKUPriceLadderingData>>(
                $"{_appConfigs.ApiBaseUrl}SKUPriceLaddering/GetApplicablePriceLaddering?broadCustomerClassification={broadCustomerClassification}"
                + $"&date={date.ToString("yyyy-MM-dd")}",
                HttpMethod.Post, productCategoryIds);

        return apiResponse != null && apiResponse.IsSuccess ? apiResponse.Data : default;
    }

    public async Task<List<ISKUPriceLadderingData>> ApplyPriceLaddering(string broadCustomerClassification, DateTime date, List<int>? productCategoryIds = null)
    {
        List<int> requiredProductCategoryIds;
        if (HasFullData)
        {
            if (productCategoryIds != null && productCategoryIds.Any())
            {
                return SKUPriceLadderingDatas.FindAll(e => productCategoryIds.Contains(e.ProductCategoryId));
            }
            else return SKUPriceLadderingDatas;
        }
        else
        {
            if (productCategoryIds != null && productCategoryIds.Any())
            {
                requiredProductCategoryIds = productCategoryIds.FindAll(e => !SKUPriceLadderingDatas.Select(_ => _.ProductCategoryId).Contains( e));
                if (requiredProductCategoryIds.Any())
                {
                    var skuPriceLadderingData = await GetApplicablePriceLaddering(broadCustomerClassification, date, requiredProductCategoryIds);
                    if (skuPriceLadderingData != null && skuPriceLadderingData.Any())
                        SKUPriceLadderingDatas.AddRange(skuPriceLadderingData);
                }
                return SKUPriceLadderingDatas.FindAll(e => productCategoryIds.Contains(e.ProductCategoryId));
            }
            var skuPriceLadderingData1 = await GetApplicablePriceLaddering(broadCustomerClassification, date, default);
            if (skuPriceLadderingData1 != null && skuPriceLadderingData1.Any())
                SKUPriceLadderingDatas.AddRange(skuPriceLadderingData1);
            HasFullData = true;
            return SKUPriceLadderingDatas;
        }

    }
}
