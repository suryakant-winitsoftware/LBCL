using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Winit.Modules.SKU.Model.Interfaces;
using Winit.Shared.Models.Common;
using Winit.Shared.Models.Enums;

namespace Winit.Modules.SKU.BL.Interfaces;

public interface ISKUPriceBL
{
    Task<PagedResponse<Winit.Modules.SKU.Model.Interfaces.ISKUPrice>> SelectAllSKUPriceDetails(List<SortCriteria> sortCriterias, int pageNumber,
        int pageSize, List<FilterCriteria> filterCriterias, bool isCountRequired, string? type = null);
    Task<PagedResponse<Winit.Modules.SKU.Model.Interfaces.ISKUPrice>> SelectAllSKUPriceDetailsByBroadClassification(List<SortCriteria> sortCriterias, int pageNumber,
        int pageSize, List<FilterCriteria> filterCriterias, bool isCountRequired, string broadClassification, string branchUID, string? type = null);
    Task<PagedResponse<Winit.Modules.SKU.Model.Interfaces.ISKUPrice>> SelectAllSKUPriceDetailsV1(List<SortCriteria> sortCriterias, int pageNumber,
        int pageSize, List<FilterCriteria> filterCriterias, bool isCountRequired);
    Task<Winit.Modules.SKU.Model.Interfaces.ISKUPrice> SelectSKUPriceByUID(string UID);

    Task<int> CreateSKUPrice(Winit.Modules.SKU.Model.Interfaces.ISKUPrice sKUPrice);
    Task<int> UpdateSKUPrice(Winit.Modules.SKU.Model.Interfaces.ISKUPrice sKUPrice);
    Task<int> UpdateSKUPriceList(List<Winit.Modules.SKU.Model.Classes.SKUPrice> sKUPrice);
    Task<int> DeleteSKUPrice(string UID);
    Task<IEnumerable<(ISKUPriceView, int)>> SelectSKUPriceViewByUID(List<SortCriteria> sortCriterias, int pageNumber,
        int pageSize, List<FilterCriteria> filterCriterias, bool isCountRequired, string skuPriceUID);
    Task<int> CreateSKUPriceView(Winit.Modules.SKU.Model.Classes.SKUPriceViewDTO sKUPriceViewDTO);
    Task<int> UpdateSKUPriceView(Winit.Modules.SKU.Model.Classes.SKUPriceViewDTO sKUPriceViewDTO);
    Task<int> CreateStandardPriceForSKU(string skuUID);
    Task<List<string>> GetApplicablePriceListByStoreUID(string storeUID, string storeType);
    Task<PagedResponse<Winit.Modules.SKU.Model.Interfaces.ISKUPrice>> SelectAllSKUPriceDetails_BySKUUIDs(List<SortCriteria> sortCriterias, int pageNumber,
        int pageSize, List<FilterCriteria> filterCriterias, bool isCountRequired, List<string> skuUIDs);
}
