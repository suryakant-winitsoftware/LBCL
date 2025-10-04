using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Winit.Modules.SKU.BL.Interfaces;
using Winit.Modules.SKU.Model.Classes;
using Winit.Modules.SKU.Model.Interfaces;
using Winit.Shared.Models.Common;
using Winit.Shared.Models.Enums;

namespace Winit.Modules.SKU.BL.Classes
{
    public class SKUGroupTypeBL : ISKUGroupTypeBL
    {
        protected readonly DL.Interfaces.ISKUGroupTypeDL _skuGroupTypeDL = null;
        public SKUGroupTypeBL(DL.Interfaces.ISKUGroupTypeDL skuGroupTypeDL)
        {
            _skuGroupTypeDL = skuGroupTypeDL;
        }
        public async Task<PagedResponse<Winit.Modules.SKU.Model.Interfaces.ISKUGroupType>> SelectAllSKUGroupTypeDetails(List<SortCriteria> sortCriterias, int pageNumber,
             int pageSize, List<FilterCriteria> filterCriterias, bool isCountRequired)
        {
            return await _skuGroupTypeDL.SelectAllSKUGroupTypeDetails(sortCriterias, pageNumber, pageSize, filterCriterias, isCountRequired);
        }
        public async Task<Winit.Modules.SKU.Model.Interfaces.ISKUGroupType> SelectSKUGroupTypeByUID(string UID)
        {
            return await _skuGroupTypeDL.SelectSKUGroupTypeByUID(UID);
        }
        public async Task<int> CreateSKUGroupType(Winit.Modules.SKU.Model.Interfaces.ISKUGroupType sKUGroupType)
        {
            return await _skuGroupTypeDL.CreateSKUGroupType(sKUGroupType);
        }

        public async Task<int> UpdateSKUGroupType(Winit.Modules.SKU.Model.Interfaces.ISKUGroupType sKUGroupType)
        {
            return await _skuGroupTypeDL.UpdateSKUGroupType(sKUGroupType);
        }

        public async Task<int> DeleteSKUGroupTypeByUID(string UID)
        {
            return await _skuGroupTypeDL.DeleteSKUGroupTypeByUID(UID);
        }
        public async Task<IEnumerable<Winit.Modules.SKU.Model.Interfaces.ISKUGroupTypeView>> SelectSKUGroupTypeView()
        {
            return await _skuGroupTypeDL.SelectSKUGroupTypeView();
        }
        public async Task<ISKUAttributeLevel> SelectSKUAttributeDDL()
        {
            return await _skuGroupTypeDL.SelectSKUAttributeDDL();
        }
    }
}
