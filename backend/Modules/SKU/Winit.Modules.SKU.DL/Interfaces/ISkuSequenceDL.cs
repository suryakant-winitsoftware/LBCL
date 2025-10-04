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
    public interface ISkuSequenceDL
    {
        Task<PagedResponse<Winit.Modules.SKU.Model.Interfaces.ISkuSequence>> SelectAllSkuSequenceDetails(List<SortCriteria> sortCriterias, int pageNumber,
               int pageSize, List<FilterCriteria> filterCriterias, bool isCountRequired, string SeqType);

        Task<int> CreateGeneralSKUSequenceForSKU(string BUOrgUID, string SKUUID);
        Task<int> CUDSkuSequence(List<Winit.Modules.SKU.Model.Classes.SkuSequence> skuSequencesList);
    }
}
