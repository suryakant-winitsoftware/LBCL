using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Winit.Shared.Models.Common;
using Winit.Shared.Models.Enums;

namespace Winit.Modules.SKU.BL.Interfaces
{
    public interface ISKUPriceListBL
    {
        Task<PagedResponse<Winit.Modules.SKU.Model.Interfaces.ISKUPriceList>> SelectAllSKUPriceListDetails(List<SortCriteria> sortCriterias, int pageNumber,
            int pageSize, List<FilterCriteria> filterCriterias, bool isCountRequired);
        Task<Winit.Modules.SKU.Model.Interfaces.ISKUPriceList> SelectSKUPriceListByUID(string UID);

        Task<int> CreateSKUPriceList(Winit.Modules.SKU.Model.Interfaces.ISKUPriceList sKUPriceList);
        Task<int> UpdateSKUPriceList(Winit.Modules.SKU.Model.Interfaces.ISKUPriceList sKUPriceList);
        Task<int> DeleteSKUPriceList(string UID);

        Task<IEnumerable<Winit.Modules.SKU.Model.Interfaces.IBuyPrice>> PopulateBuyPrice(string OrgUID);
    }
}
