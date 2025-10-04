using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Winit.Shared.Models.Common;
using Winit.Shared.Models.Enums;

namespace Winit.Modules.RSSMailQueue.BL.Interfaces
{
    public interface IRSSMailQueueBL
    {
        Task<PagedResponse<Winit.Modules.RSSMailQueue.Model.Interfaces.IRSSMailQueue>> SelectAllRSSMailQueueDetails(List<SortCriteria> sortCriterias, int pageNumber,
          int pageSize, List<FilterCriteria> filterCriterias, bool isCountRequired);
        Task<Winit.Modules.RSSMailQueue.Model.Interfaces.IRSSMailQueue> SelectRSSMailQueueByUID(string UID);
        Task<int> CreateRSSMailQueue(Winit.Modules.RSSMailQueue.Model.Interfaces.IRSSMailQueue rSSMailQueueDetails);
        Task<int> UpdateRSSMailQueue(Winit.Modules.RSSMailQueue.Model.Interfaces.IRSSMailQueue rSSMailQueueDetails);
        Task<int> DeleteRSSMailQueue(string UID);
    }
}
