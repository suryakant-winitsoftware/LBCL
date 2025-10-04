using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Winit.Modules.RSSMailQueue.BL.Interfaces;
using Winit.Modules.RSSMailQueue.DL.Interfaces;
using Winit.Modules.RSSMailQueue.Model.Interfaces;
using Winit.Shared.Models.Common;
using Winit.Shared.Models.Enums;

namespace Winit.Modules.RSSMailQueue.BL.Classes
{
    public class RSSMailQueueBL : IRSSMailQueueBL
    {
        private readonly IRSSMailQueueDL _rSSMailQueueDL;
        public RSSMailQueueBL(IRSSMailQueueDL rSSMailQueueDL)
        {
            _rSSMailQueueDL = rSSMailQueueDL;
        }
        public async Task<int> CreateRSSMailQueue(Winit.Modules.RSSMailQueue.Model.Interfaces.IRSSMailQueue rSSMailQueueDetails)
        {
            return await _rSSMailQueueDL.CreateRSSMailQueue(rSSMailQueueDetails);
        }

        public Task<int> DeleteRSSMailQueue(string UID)
        {
            throw new NotImplementedException();
        }

        public Task<PagedResponse<IRSSMailQueue>> SelectAllRSSMailQueueDetails(List<SortCriteria> sortCriterias, int pageNumber, int pageSize, List<FilterCriteria> filterCriterias, bool isCountRequired)
        {
            throw new NotImplementedException();
        }

        public Task<IRSSMailQueue> SelectRSSMailQueueByUID(string UID)
        {
            throw new NotImplementedException();
        }

        public Task<int> UpdateRSSMailQueue(IRSSMailQueue rSSMailQueueDetails)
        {
            throw new NotImplementedException();
        }
    }
}
