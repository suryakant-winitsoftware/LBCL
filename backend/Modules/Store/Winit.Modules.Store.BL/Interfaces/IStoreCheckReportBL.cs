using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Winit.Shared.Models.Common;
using Winit.Shared.Models.Enums;

namespace Winit.Modules.Store.BL.Interfaces
{
    public interface IStoreCheckReportBL
    {
        Task<PagedResponse<Winit.Modules.Store.Model.Interfaces.IStoreCheckReport>> GetStoreCheckReportDetails(List<SortCriteria> sortCriterias, int pageNumber,
          int pageSize, List<FilterCriteria> filterCriterias, bool isCountRequired);

        Task<List<Winit.Modules.Store.Model.Interfaces.IStoreCheckReportItem>> GetStoreCheckReportItems(string uid);
    }
}
