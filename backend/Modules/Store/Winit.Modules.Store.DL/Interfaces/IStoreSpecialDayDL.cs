using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Winit.Shared.Models.Common;
using Winit.Shared.Models.Constants;
using Winit.Shared.Models.Enums;

namespace Winit.Modules.Store.DL.Interfaces
{
    public interface IStoreSpecialDayDL
    {
        Task<PagedResponse<Winit.Modules.Store.Model.Interfaces.IStoreSpecialDay>> SelectAllStoreSpecialDay(List<SortCriteria> sortCriterias, int pageNumber,
              int pageSize, List<FilterCriteria> filterCriterias, bool isCountRequired);
        Task<Winit.Modules.Store.Model.Interfaces.IStoreSpecialDay> SelectAllStoreSpecialDayByUID(string UID);
        Task<int> CreateStoreSpecialDay(Winit.Modules.Store.Model.Interfaces.IStoreSpecialDay storeSpecialDay);
        Task<int> UpdateStoreSpecialDay(Winit.Modules.Store.Model.Interfaces.IStoreSpecialDay UpdatestoreSpecialDay);
        Task<int> DeleteStoreSpecialDay(string UID);
    }
}
