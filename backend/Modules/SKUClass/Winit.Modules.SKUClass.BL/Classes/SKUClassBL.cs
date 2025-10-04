using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Winit.Modules.SKUClass.BL.Interfaces;
using Winit.Shared.Models.Common;
using Winit.Shared.Models.Enums;

namespace Winit.Modules.SKUClass.BL.Classes
{
    public class SKUClassBL:ISKUClassBL
    {
        protected readonly DL.Interfaces.ISKUClassDL _skuClassBL = null;
        public SKUClassBL(DL.Interfaces.ISKUClassDL skuClassBL)
        {
            _skuClassBL = skuClassBL;
        }
        public async Task<PagedResponse<Winit.Modules.SKUClass.Model.Interfaces.ISKUClass>> SelectAllSKUClassDetails(List<SortCriteria> sortCriterias, int pageNumber,
            int pageSize, List<FilterCriteria> filterCriterias, bool isCountRequired)
        {
            return await _skuClassBL.SelectAllSKUClassDetails(sortCriterias, pageNumber, pageSize, filterCriterias, isCountRequired);
        }
        public async Task<Winit.Modules.SKUClass.Model.Interfaces.ISKUClass> GetSKUClassByUID(string UID)
        {
            return await _skuClassBL.GetSKUClassByUID(UID);
        }
        public async Task<int> CreateSKUClass(Winit.Modules.SKUClass.Model.Interfaces.ISKUClass createSKUClass)
        {
            return await _skuClassBL.CreateSKUClass(createSKUClass);
        }
        public async Task<int> UpdateSKUClass(Winit.Modules.SKUClass.Model.Interfaces.ISKUClass updateSKUClass)
        {
            return await _skuClassBL.UpdateSKUClass(updateSKUClass);
        }
        public async Task<int> DeleteSKUClass(string UID)
        {
            return await _skuClassBL.DeleteSKUClass(UID);
        }
    }
}
