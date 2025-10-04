using Winit.Modules.PriceLadder.Model.Interfaces;
using Winit.Modules.SKU.Model.Interfaces;
using Winit.Shared.Models.Common;
using Winit.Shared.Models.Enums;

namespace Winit.Modules.PriceLadder.BL.Interfaces;

public interface ISKUPriceLadderingBL
{
    Task<List<ISKUPriceLadderingData>> GetApplicablePriceLaddering(string broadCustomerClassification,
            DateTime date, List<int>? productCategoryIds = null);
    Task<PagedResponse<IPriceLadderingItemView>> SelectAllThePriceLaddering(
     List<SortCriteria> sortCriterias,
     int pageNumber,
     int pageSize,
     List<FilterCriteria> filterCriterias,
     bool isCountRequired
     );
    Task<PagedResponse<IPriceLaddering>> GetPriceLadders(
             List<SortCriteria> sortCriterias,
             int pageNumber,
             int pageSize,
             List<FilterCriteria> filterCriterias,
             bool isCountRequired
                     );
    Task<List<IPriceLaddering>> GetRelatedData(string operatingUnit, string division, string branch, string salesOffice, string broadCustomerClassification);
    Task<List<ISKU>> GetSkuDetailsFromProductCategoryId(int productCategoryId);
    Task<List<int>> GetProductCategoryIdsByStoreUID(string storeUID, DateTime date, string broadClassification = null, string branchUID = null);
}
