using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Winit.Modules.SKU.Model.Interfaces;
using Winit.Shared.Models.Common;
using Winit.Shared.Models.Enums;

namespace Winit.Modules.SKU.DL.Interfaces
{
    public interface ISKUGroupTypeDL
    {
        Task<PagedResponse<Winit.Modules.SKU.Model.Interfaces.ISKUGroupType>> SelectAllSKUGroupTypeDetails(List<SortCriteria> sortCriterias, int pageNumber,
         int pageSize, List<FilterCriteria> filterCriterias, bool isCountRequired);
        Task<Winit.Modules.SKU.Model.Interfaces.ISKUGroupType> SelectSKUGroupTypeByUID(string UID);

        Task<int> CreateSKUGroupType(Winit.Modules.SKU.Model.Interfaces.ISKUGroupType sKUGroupType);
        Task<int> UpdateSKUGroupType(Winit.Modules.SKU.Model.Interfaces.ISKUGroupType sKUGroupType);
        Task<int> DeleteSKUGroupTypeByUID(string UID);
        Task<IEnumerable<Winit.Modules.SKU.Model.Interfaces.ISKUGroupTypeView>> SelectSKUGroupTypeView();
        Task<ISKUAttributeLevel> SelectSKUAttributeDDL();
    }
}
