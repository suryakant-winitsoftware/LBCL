using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Winit.Modules.SKU.BL.Interfaces;
using Winit.Modules.SKU.DL.Interfaces;
using Winit.Modules.SKU.Model.Classes;
using Winit.Shared.Models.Common;
using Winit.Shared.Models.Enums;

namespace Winit.Modules.SKU.BL.Classes
{
    public class SKUUOMBL : ISKUUOMBL
    {
        protected readonly DL.Interfaces.ISKUUOMDL _skuUOMDL = null;
        public SKUUOMBL(DL.Interfaces.ISKUUOMDL skuUOMDL)
        {
            _skuUOMDL = skuUOMDL;
        }
        public async Task<PagedResponse<Winit.Modules.SKU.Model.Interfaces.ISKUUOM>> SelectAllSKUUOMDetails(List<SortCriteria> sortCriterias, int pageNumber,
             int pageSize, List<FilterCriteria> filterCriterias, bool isCountRequired)
        {
            return await _skuUOMDL.SelectAllSKUUOMDetails(sortCriterias, pageNumber, pageSize, filterCriterias, isCountRequired);
        }
        public async Task<Winit.Modules.SKU.Model.Interfaces.ISKUUOM> SelectSKUUOMByUID(string UID)
        {
            return await _skuUOMDL.SelectSKUUOMByUID(UID);
        }
        public async Task<int> CreateSKUUOM(Winit.Modules.SKU.Model.Interfaces.ISKUUOM sKUUOM)
        {
            return await _skuUOMDL.CreateSKUUOM(sKUUOM);
        }

        public async Task<int> UpdateSKUUOM(Winit.Modules.SKU.Model.Interfaces.ISKUUOM sKUUOM)
        {
            return await _skuUOMDL.UpdateSKUUOM(sKUUOM);
        }

        public async Task<int> DeleteSKUUOMByUID(string UID)
        {
            return await _skuUOMDL.DeleteSKUUOMByUID(UID);
        }

        public async Task<int> UpdateSKUForBaseAndOuterUOMs(Winit.Modules.SKU.Model.Interfaces.ISKUUOM sKUUOM)
        {
            return await _skuUOMDL.UpdateSKUForBaseAndOuterUOMs(sKUUOM);
        }
    }
}
