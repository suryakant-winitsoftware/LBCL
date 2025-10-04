using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Winit.Shared.Models.Common;
using Winit.Shared.Models.Enums;

namespace Winit.Modules.AwayPeriod.DL.Interfaces
{
    public interface IAwayPeriodDL
    {
        Task<PagedResponse<Winit.Modules.AwayPeriod.Model.Interfaces.IAwayPeriod>> GetAwayPeriodDetails(List<SortCriteria> sortCriterias, int pageNumber,
            int pageSize, List<FilterCriteria> filterCriterias, bool isCountRequired);
        Task<Winit.Modules.AwayPeriod.Model.Interfaces.IAwayPeriod> GetAwayPeriodDetailsByUID(string UID);
        Task<int> CreateAwayPeriodDetails(Winit.Modules.AwayPeriod.Model.Interfaces.IAwayPeriod awayPeriod);
        Task<int> UpdateAwayPeriodDetails(Winit.Modules.AwayPeriod.Model.Interfaces.IAwayPeriod awayPeriodDetails);
        Task<int> DeleteAwayPeriodDetail(String UID);
    }
}
