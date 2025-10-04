using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Winit.Modules.ListHeader.Model.Interfaces;
using Winit.Shared.Models.Common;
using Winit.Shared.Models.Enums;

namespace Winit.Modules.ListHeader.BL.Interfaces
{
    public interface IListHeaderBL
    {

        Task<PagedResponse<IListHeader>> GetListHeaders(List<SortCriteria> sortCriterias, int pageNumber,
           int pageSize, List<FilterCriteria> filterCriterias, bool isCountRequired);
        Task<PagedResponse<Winit.Modules.ListHeader.Model.Interfaces.IListItem>> GetListItemsByCodes(List<string> Codes, bool isCountRequired);
        Task<PagedResponse<Winit.Modules.ListHeader.Model.Interfaces.IListItem>> GetListItemsByListHeaderCodes(List<string> codes, List<SortCriteria> sortCriterias, int pageNumber, int pageSize,
     List<FilterCriteria> filterCriterias, bool isCountRequired);
        Task<int> CreateListItem(Winit.Modules.ListHeader.Model.Interfaces.IListItem listItem);
        Task<int> UpdateListItem(Winit.Modules.ListHeader.Model.Interfaces.IListItem listItem);
        Task<int> DeleteListItemByUID(string UID);

        Task<IEnumerable<IListItem>> GetListItemsByHeaderUID(string headerUID);
        Task<IListItem> GetListItemsByUID(string UID);
    }
}
