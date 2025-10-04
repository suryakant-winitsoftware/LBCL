using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Winit.Modules.SKU.BL.Interfaces;
using Winit.Modules.SKU.Model.Classes;
using Winit.Shared.Models.Common;
using Winit.Shared.Models.Enums;

namespace Winit.Modules.SKU.BL.Classes
{
    public class SKUPriceListBL:ISKUPriceListBL
    {
        protected readonly DL.Interfaces.ISKUPriceListDL _SKUPriceListDL = null;
        public SKUPriceListBL(DL.Interfaces.ISKUPriceListDL SKUPriceListDL)
        {
            _SKUPriceListDL = SKUPriceListDL;
        }
        public async Task<PagedResponse<Winit.Modules.SKU.Model.Interfaces.ISKUPriceList>> SelectAllSKUPriceListDetails(List<SortCriteria> sortCriterias, int pageNumber,
             int pageSize, List<FilterCriteria> filterCriterias, bool isCountRequired)
        {
            return await _SKUPriceListDL.SelectAllSKUPriceListDetails(sortCriterias, pageNumber, pageSize, filterCriterias, isCountRequired);
        }
        public async Task<Winit.Modules.SKU.Model.Interfaces.ISKUPriceList> SelectSKUPriceListByUID(string UID)
        {
            return await _SKUPriceListDL.SelectSKUPriceListByUID(UID);
        }
        public async Task<int> CreateSKUPriceList(Winit.Modules.SKU.Model.Interfaces.ISKUPriceList createSKUPriceList)
        {
            return await _SKUPriceListDL.CreateSKUPriceList(createSKUPriceList);
        }

        public async Task<int> UpdateSKUPriceList(Winit.Modules.SKU.Model.Interfaces.ISKUPriceList updateSKUPriceList)
        {
            return await _SKUPriceListDL.UpdateSKUPriceList(updateSKUPriceList);
        }

        public async Task<int> DeleteSKUPriceList(string UID)
        {
            return await _SKUPriceListDL.DeleteSKUPriceList(UID);
        }
        public async Task<IEnumerable<Winit.Modules.SKU.Model.Interfaces.IBuyPrice>> PopulateBuyPrice(string OrgUID)
        {
            IEnumerable<Winit.Modules.SKU.Model.Interfaces.IBuyPrice> buyPricesList= await _SKUPriceListDL.PopulateBuyPrice(OrgUID);
            return buyPricesList;
        }
    }
}
