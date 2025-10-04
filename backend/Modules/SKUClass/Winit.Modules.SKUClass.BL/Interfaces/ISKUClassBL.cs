using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Winit.Shared.Models.Common;
using Winit.Shared.Models.Enums;

namespace Winit.Modules.SKUClass.BL.Interfaces
{
    public interface ISKUClassBL
    {
        Task<PagedResponse<Winit.Modules.SKUClass.Model.Interfaces.ISKUClass>> SelectAllSKUClassDetails(List<SortCriteria> sortCriterias, int pageNumber,
             int pageSize, List<FilterCriteria> filterCriterias, bool isCountRequired);
        Task<Winit.Modules.SKUClass.Model.Interfaces.ISKUClass> GetSKUClassByUID(string UID);
        Task<int> CreateSKUClass(Winit.Modules.SKUClass.Model.Interfaces.ISKUClass createSKUClass);
        Task<int> UpdateSKUClass(Winit.Modules.SKUClass.Model.Interfaces.ISKUClass updateSKUClass);
        Task<int> DeleteSKUClass(string UID);
    }
}
