using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Winit.Shared.Models.Common;
using Winit.Shared.Models.Enums;

namespace Winit.Modules.SKU.DL.Interfaces
{
    public interface ISKUUOMDL
    {
        Task<PagedResponse<Winit.Modules.SKU.Model.Interfaces.ISKUUOM>> SelectAllSKUUOMDetails(List<SortCriteria> sortCriterias, int pageNumber,
           int pageSize, List<FilterCriteria> filterCriterias, bool isCountRequired);
        Task<Winit.Modules.SKU.Model.Interfaces.ISKUUOM> SelectSKUUOMByUID(string UID);

        Task<int> CreateSKUUOM(Winit.Modules.SKU.Model.Interfaces.ISKUUOM sKUUOM);
        Task<int> UpdateSKUUOM(Winit.Modules.SKU.Model.Interfaces.ISKUUOM sKUUOM);
        Task<int> DeleteSKUUOMByUID(string UID);
        Task<int> UpdateSKUForBaseAndOuterUOMs(Winit.Modules.SKU.Model.Interfaces.ISKUUOM sKUUOM);
    }
}
