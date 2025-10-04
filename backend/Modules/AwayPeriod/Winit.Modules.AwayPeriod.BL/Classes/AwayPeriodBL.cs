using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Winit.Shared.Models.Common;
using Winit.Shared.Models.Enums;

namespace Winit.Modules.AwayPeriod.BL.Classes
{
    public class AwayPeriodBL: AwayPeriodBaseBL, Interfaces.IAwayPeriodBL
    {
        protected readonly DL.Interfaces.IAwayPeriodDL _awayPeriodDL = null;
        public AwayPeriodBL(DL.Interfaces.IAwayPeriodDL awayPeriodDL)
        {
            _awayPeriodDL = awayPeriodDL;   
        }
        public async Task<PagedResponse<Winit.Modules.AwayPeriod.Model.Interfaces.IAwayPeriod>> GetAwayPeriodDetails(List<SortCriteria> sortCriterias, int pageNumber,
            int pageSize, List<FilterCriteria> filterCriterias, bool isCountRequired)
        {
            return await _awayPeriodDL.GetAwayPeriodDetails(sortCriterias, pageNumber, pageSize, filterCriterias,isCountRequired);
        }

        public async Task<Winit.Modules.AwayPeriod.Model.Interfaces.IAwayPeriod> GetAwayPeriodDetailsByUID(string UID)
        {
            return await _awayPeriodDL.GetAwayPeriodDetailsByUID(UID);
        }
        public async Task<int> CreateAwayPeriodDetails(Winit.Modules.AwayPeriod.Model.Interfaces.IAwayPeriod awayPeriod)
        {
            return await _awayPeriodDL.CreateAwayPeriodDetails(awayPeriod);
        }

        public async Task<int> UpdateAwayPeriodDetails(Winit.Modules.AwayPeriod.Model.Interfaces.IAwayPeriod awayPeriodDetails)
        {
            return await _awayPeriodDL.UpdateAwayPeriodDetails(awayPeriodDetails);
        }

        public async Task<int> DeleteAwayPeriodDetail(String UID)
        {
            return await _awayPeriodDL.DeleteAwayPeriodDetail(UID);
        }
    }
}
