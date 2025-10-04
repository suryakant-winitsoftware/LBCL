using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Winit.Modules.UOM.BL.Interfaces;
using Winit.Shared.Models.Common;
using Winit.Shared.Models.Enums;

namespace Winit.Modules.UOM.BL.Classes
{
    public class UOMTypeBL : IUOMTypeBL
    {
        protected readonly DL.Interfaces.IUOMTypeDL _uomTypeBL;
        public UOMTypeBL(DL.Interfaces.IUOMTypeDL uomTypeBL)
        {
            _uomTypeBL = uomTypeBL;
        }
        public async Task<PagedResponse<Winit.Modules.UOM.Model.Interfaces.IUOMType>> SelectAllUOMTypeDetails(List<SortCriteria> sortCriterias, int pageNumber,
            int pageSize, List<FilterCriteria> filterCriterias, bool isCountRequired)
        {
            return await _uomTypeBL.SelectAllUOMTypeDetails(sortCriterias, pageNumber, pageSize, filterCriterias, isCountRequired);
        }
       
    }
}
