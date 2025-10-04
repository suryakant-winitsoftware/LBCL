using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Winit.Modules.SKU.BL.Interfaces;
using Winit.Modules.SKU.DL.Interfaces;
using Winit.Modules.SKU.Model.Classes;
using Winit.Modules.SKU.Model.Interfaces;
using Winit.Shared.CommonUtilities.Extensions;
using Winit.Shared.Models.Common;
using Winit.Shared.Models.Enums;

namespace Winit.Modules.SKU.BL.Classes
{
    public class SKUPriceBL : ISKUPriceBL
    {
        protected readonly DL.Interfaces.ISKUPriceDL _SKUPriceDL = null;
        IServiceProvider _serviceProvider = null;
        public SKUPriceBL(DL.Interfaces.ISKUPriceDL sKUPriceDL, IServiceProvider serviceProvider)
        {
            _SKUPriceDL = sKUPriceDL;
            _serviceProvider = serviceProvider;
        }
        public async Task<PagedResponse<Winit.Modules.SKU.Model.Interfaces.ISKUPrice>> SelectAllSKUPriceDetails(List<SortCriteria> sortCriterias, int pageNumber,
            int pageSize, List<FilterCriteria> filterCriterias, bool isCountRequired, string? type = null)
        {
            return await _SKUPriceDL.SelectAllSKUPriceDetails(sortCriterias, pageNumber, pageSize, filterCriterias, isCountRequired, type);
        }
        public async Task<PagedResponse<Winit.Modules.SKU.Model.Interfaces.ISKUPrice>> SelectAllSKUPriceDetailsByBroadClassification(List<SortCriteria> sortCriterias, int pageNumber,
            int pageSize, List<FilterCriteria> filterCriterias, bool isCountRequired, string broadClassification, string branchUID, string? type = null)
        {
            return await _SKUPriceDL.SelectAllSKUPriceDetailsByBroadClassification(sortCriterias, pageNumber, pageSize, filterCriterias, isCountRequired, broadClassification, branchUID, type);
        }
        public async Task<PagedResponse<Winit.Modules.SKU.Model.Interfaces.ISKUPrice>> SelectAllSKUPriceDetailsV1(List<SortCriteria> sortCriterias, int pageNumber,
            int pageSize, List<FilterCriteria> filterCriterias, bool isCountRequired)
        {
            return await _SKUPriceDL.SelectAllSKUPriceDetailsV1(sortCriterias, pageNumber, pageSize, filterCriterias, isCountRequired);
        }
        public async Task<Winit.Modules.SKU.Model.Interfaces.ISKUPrice> SelectSKUPriceByUID(string UID)
        {
            return await _SKUPriceDL.SelectSKUPriceByUID(UID);
        }
        public async Task<int> CreateSKUPrice(Winit.Modules.SKU.Model.Interfaces.ISKUPrice createSKUPrice)
        {
            return await _SKUPriceDL.CreateSKUPrice(createSKUPrice);
        }

        public async Task<int> UpdateSKUPrice(Winit.Modules.SKU.Model.Interfaces.ISKUPrice updateSKUPrice)
        {
            return await _SKUPriceDL.UpdateSKUPrice(updateSKUPrice);
        }
        public async Task<int> UpdateSKUPriceList(List<Winit.Modules.SKU.Model.Classes.SKUPrice> updateSKUPrice)
        {
            return await _SKUPriceDL.UpdateSKUPriceList(updateSKUPrice);
        }

        public async Task<int> DeleteSKUPrice(string UID)
        {
            return await _SKUPriceDL.DeleteSKUPrice(UID);
        }
        public async Task<IEnumerable<(ISKUPriceView, int)>> SelectSKUPriceViewByUID(List<SortCriteria> sortCriterias, int pageNumber,
            int pageSize, List<FilterCriteria> filterCriterias, bool isCountRequired, string skuPriceUID)
        {
            var (SkuPriceList, sKUPrices, totalCount) = await _SKUPriceDL.SelectSKUPriceViewByUID(sortCriterias, pageNumber, pageSize, filterCriterias, isCountRequired, skuPriceUID);
            Winit.Modules.SKU.Model.Interfaces.ISKUPriceView sKUPriceView = _serviceProvider.CreateInstance<Winit.Modules.SKU.Model.Interfaces.ISKUPriceView>();
            if (SkuPriceList != null && SkuPriceList.Count > 0)
            {
                sKUPriceView.SKUPriceGroup = SkuPriceList.FirstOrDefault();
            }
            if (sKUPrices != null && sKUPrices.Count > 0)
            {
                sKUPriceView.SKUPriceList = sKUPrices;
            }
            return new List<(ISKUPriceView, int)>
            {
                (sKUPriceView, totalCount)
            };

        }
        public async Task<int> CreateSKUPriceView(Winit.Modules.SKU.Model.Classes.SKUPriceViewDTO sKUPriceViewDTO)
        {
            return await _SKUPriceDL.CreateSKUPriceView(sKUPriceViewDTO);
        }

        public async Task<int> UpdateSKUPriceView(Winit.Modules.SKU.Model.Classes.SKUPriceViewDTO sKUPriceViewDTO)
        {
            return await _SKUPriceDL.UpdateSKUPriceView(sKUPriceViewDTO);
        }
        public async Task<int> CreateStandardPriceForSKU(string skuUID)
        {
            return await _SKUPriceDL.CreateStandardPriceForSKU(skuUID);
        }
        public async Task<List<string>> GetApplicablePriceListByStoreUID(string storeUID, string storeType)
        {
            return await _SKUPriceDL.GetApplicablePriceListByStoreUID(storeUID, storeType);
        }
        public async Task<PagedResponse<Winit.Modules.SKU.Model.Interfaces.ISKUPrice>> SelectAllSKUPriceDetails_BySKUUIDs(List<SortCriteria> sortCriterias, int pageNumber,
            int pageSize, List<FilterCriteria> filterCriterias, bool isCountRequired, List<string> skuUIDs)
        {
            return await _SKUPriceDL.SelectAllSKUPriceDetails_BySKUUIDs(sortCriterias, pageNumber, pageSize, filterCriterias, isCountRequired, skuUIDs);
        }
    }
}
