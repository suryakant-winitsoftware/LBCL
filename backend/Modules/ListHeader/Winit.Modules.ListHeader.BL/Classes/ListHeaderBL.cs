using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Winit.Modules.ListHeader.BL.Interfaces;
using Winit.Modules.ListHeader.Model.Interfaces;
using Winit.Shared.Models.Common;
using Winit.Shared.Models.Enums;

namespace Winit.Modules.ListHeader.BL.Classes
{
    public class ListHeaderBL : IListHeaderBL
    {
        protected readonly DL.Interfaces.IListHeaderDL _listHeaderDL = null;
        public ListHeaderBL(DL.Interfaces.IListHeaderDL listHeaderDL)
        {
            _listHeaderDL = listHeaderDL;
        }
        public async Task<PagedResponse<Winit.Modules.ListHeader.Model.Interfaces.IListHeader>> GetListHeaders(List<SortCriteria> sortCriterias, int pageNumber,
           int pageSize, List<FilterCriteria> filterCriterias, bool isCountRequired)
        {
            return await _listHeaderDL.GetListHeaders(sortCriterias, pageNumber, pageSize, filterCriterias,isCountRequired);
        }
     
        public async Task<PagedResponse<Winit.Modules.ListHeader.Model.Interfaces.IListItem>> GetListItemsByCodes(List<string> Codes, bool isCountRequired)
        {
            return await _listHeaderDL.GetListItemsByCodes(Codes, isCountRequired);
        }
        public async Task<PagedResponse<Winit.Modules.ListHeader.Model.Interfaces.IListItem>> GetListItemsByListHeaderCodes(List<string> codes, List<SortCriteria> sortCriterias, int pageNumber, int pageSize,
     List<FilterCriteria> filterCriterias, bool isCountRequired)
        {
            return await _listHeaderDL.GetListItemsByListHeaderCodes(codes,sortCriterias, pageNumber, pageSize, filterCriterias, isCountRequired);
        }
        public async Task<int> CreateListItem(Winit.Modules.ListHeader.Model.Interfaces.IListItem listItem)
        {
            return await _listHeaderDL.CreateListItem(listItem);
        }

        public async Task<int> UpdateListItem(Winit.Modules.ListHeader.Model.Interfaces.IListItem listItem)
        {
            return await _listHeaderDL.UpdateListItem(listItem);
        }

        public async Task<int> DeleteListItemByUID(string UID)
        {
            return await _listHeaderDL.DeleteListItemByUID(UID);
        }

        public async Task<IEnumerable<IListItem>> GetListItemsByHeaderUID(string headerUID)
        {
            return await _listHeaderDL.GetListItemsByHeaderUID(headerUID);
        }
        public async Task<IListItem> GetListItemsByUID(string UID)
        {
            return await _listHeaderDL.GetListItemsByUID(UID);
        }
    }
}
