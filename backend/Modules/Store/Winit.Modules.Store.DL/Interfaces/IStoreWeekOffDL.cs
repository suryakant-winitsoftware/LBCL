using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Winit.Modules.Store.Model.Classes;
using Winit.Shared.Models.Common;
using Winit.Shared.Models.Constants;
using Winit.Shared.Models.Enums;

namespace Winit.Modules.Store.DL.Interfaces
{
    public interface IStoreWeekOffDL
    {
        Task<PagedResponse<Model.Interfaces.IStoreWeekOff>> SelectAllStoreWeekOff(List<SortCriteria> sortCriterias, int pageNumber,
         int pageSize, List<FilterCriteria> filterCriterias, bool isCountRequired);
        Task<Model.Interfaces.IStoreWeekOff> SelectStoreWeekOffByUID(string UID);
        Task<int> CreateStoreWeekOff(Model.Interfaces.IStoreWeekOff storeWeekOff);
        Task<int> UpdateStoreWeekOff(Model.Interfaces.IStoreWeekOff storeWeekOff);
        Task<int> DeleteStoreWeekOff(string UID);
    }
}
