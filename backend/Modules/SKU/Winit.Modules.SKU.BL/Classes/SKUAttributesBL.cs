using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Winit.Modules.SKU.BL.Interfaces;
using Winit.Modules.SKU.DL.Interfaces;
using Winit.Modules.SKU.Model.Classes;
using Winit.Modules.SKU.Model.UIClasses;
using Winit.Shared.Models.Common;
using Winit.Shared.Models.Enums;

namespace Winit.Modules.SKU.BL.Classes
{
    public class SKUAttributesBL : ISKUAttributesBL
    {
        protected readonly DL.Interfaces.ISKUAttributesDL _skuAttributesDL = null;
        public SKUAttributesBL(DL.Interfaces.ISKUAttributesDL skuAttributesDL)
        {
            _skuAttributesDL = skuAttributesDL;
        }
        public async Task<PagedResponse<Winit.Modules.SKU.Model.Interfaces.ISKUAttributes>> SelectAllSKUAttributesDetails(List<SortCriteria> sortCriterias, int pageNumber,
             int pageSize, List<FilterCriteria> filterCriterias, bool isCountRequired)
        {
            return await _skuAttributesDL.SelectAllSKUAttributesDetails(sortCriterias, pageNumber, pageSize, filterCriterias, isCountRequired);
        }
        public async Task<Winit.Modules.SKU.Model.Interfaces.ISKUAttributes> SelectSKUAttributesByUID(string UID)
        {
            return await _skuAttributesDL.SelectSKUAttributesByUID(UID);
        }
        public async Task<int> CreateSKUAttributes(Winit.Modules.SKU.Model.Interfaces.ISKUAttributes sKUAttributes)
        {
            return await _skuAttributesDL.CreateSKUAttributes(sKUAttributes);
        }
        public async Task<int> CreateBulkSKUAttributes(List<Winit.Modules.SKU.Model.Interfaces.ISKUAttributes> sKUAttributes)
        {
            return await _skuAttributesDL.CreateBulkSKUAttributes(sKUAttributes);
        }
        public async Task<int> CUDBulkSKUAttributes(List<Winit.Modules.SKU.Model.Interfaces.ISKUAttributes> sKUAttributesList)
        {
            return await _skuAttributesDL.CUDBulkSKUAttributes(sKUAttributesList);
        }

        public async Task<int> UpdateSKUAttributes(Winit.Modules.SKU.Model.Interfaces.ISKUAttributes sKUAttributes)
        {
            return await _skuAttributesDL.UpdateSKUAttributes(sKUAttributes);
        }

        public async Task<int> DeleteSKUAttributesByUID(string UID)
        {
            return await _skuAttributesDL.DeleteSKUAttributesByUID(UID);
        }
        public async Task<List<SKUAttributeDropdownModel>> GetSKUGroupTypeForSKuAttribute()
        {
            return await _skuAttributesDL.GetSKUGroupTypeForSKuAttribute();
        }
    }
}
