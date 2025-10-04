using Winit.Modules.PriceLadder.BL.Interfaces;
using Winit.Modules.PriceLadder.DL.Interfaces;
using Winit.Modules.PriceLadder.Model.Interfaces;
using Winit.Modules.SKU.Model.Interfaces;
using Winit.Shared.Models.Common;
using Winit.Shared.Models.Enums;

namespace Winit.Modules.PriceLadder.BL.Classes;

public class SKUPriceLadderingBL : ISKUPriceLadderingBL
{
    private readonly ISkuPriceLadderingDL _sKUPriceLadderingDL;

    public SKUPriceLadderingBL(ISkuPriceLadderingDL sKUPriceLadderingDL)
    {
        _sKUPriceLadderingDL = sKUPriceLadderingDL;
    }
    public async Task<List<ISKUPriceLadderingData>> GetApplicablePriceLaddering(string broadCustomerClassification, DateTime date, List<int>? productCategoryIds = null)
    {
        return await _sKUPriceLadderingDL.GetApplicablePriceLaddering(broadCustomerClassification,date, productCategoryIds);
    }

    public async Task<PagedResponse<IPriceLaddering>> GetPriceLadders(List<SortCriteria> sortCriterias, int pageNumber, int pageSize, List<FilterCriteria> filterCriterias, bool isCountRequired)
    {
        return await _sKUPriceLadderingDL.GetPriceLadders(
            sortCriterias,
            pageNumber,
            pageSize,
            filterCriterias,
            isCountRequired
        );
    }

    public async Task<List<IPriceLaddering>> GetRelatedData(string operatingUnit, string division, string branch, string salesOffice, string broadCustomerClassification)
    {
        return await _sKUPriceLadderingDL.GetRelatedData(operatingUnit, division, branch, salesOffice, broadCustomerClassification);
    }
    public async Task<List<ISKU>> GetSkuDetailsFromProductCategoryId(int productCategoryId)
    {
        return await _sKUPriceLadderingDL.GetSkuDetailsFromProductCategoryId(productCategoryId);
    }

    public async Task<PagedResponse<IPriceLadderingItemView>> SelectAllThePriceLaddering(
            List<SortCriteria> sortCriterias,
            int pageNumber,
            int pageSize,
            List<FilterCriteria> filterCriterias,
            bool isCountRequired)
    {
        return await _sKUPriceLadderingDL.SelectAllThePriceLaddering(
            sortCriterias,
            pageNumber,
            pageSize,
            filterCriterias,
            isCountRequired
        );
    }

    public async Task<List<int>> GetProductCategoryIdsByStoreUID(string storeUID, DateTime date, string broadClassification = null, string branchUID = null)
    {
        return await _sKUPriceLadderingDL.GetProductCategoryIdsByStoreUid(storeUID, date,broadClassification,branchUID);
    }
}
