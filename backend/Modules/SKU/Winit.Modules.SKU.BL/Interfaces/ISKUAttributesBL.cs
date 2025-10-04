using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Winit.Modules.SKU.Model.UIClasses;
using Winit.Shared.Models.Common;
using Winit.Shared.Models.Enums;

namespace Winit.Modules.SKU.BL.Interfaces
{
    public interface ISKUAttributesBL
    {
        Task<PagedResponse<Winit.Modules.SKU.Model.Interfaces.ISKUAttributes>> SelectAllSKUAttributesDetails(List<SortCriteria> sortCriterias, int pageNumber,
            int pageSize, List<FilterCriteria> filterCriterias, bool isCountRequired);
        Task<Winit.Modules.SKU.Model.Interfaces.ISKUAttributes> SelectSKUAttributesByUID(string UID);

        Task<int> CreateSKUAttributes(Winit.Modules.SKU.Model.Interfaces.ISKUAttributes sKUAttributes);
        Task<int> CreateBulkSKUAttributes(List<Winit.Modules.SKU.Model.Interfaces.ISKUAttributes> sKUAttributes);
        Task<int> CUDBulkSKUAttributes(List<Winit.Modules.SKU.Model.Interfaces.ISKUAttributes> sKUAttributesList);
        Task<int> UpdateSKUAttributes(Winit.Modules.SKU.Model.Interfaces.ISKUAttributes sKUAttributes);
        Task<int> DeleteSKUAttributesByUID(string UID);
        Task<List<SKUAttributeDropdownModel>> GetSKUGroupTypeForSKuAttribute();
    }
}
