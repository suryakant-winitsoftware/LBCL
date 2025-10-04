using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Winit.Modules.Syncing.Model;
using Winit.Modules.Syncing.Model.Interfaces;

namespace Winit.Modules.Syncing.DL.Interfaces
{
    public interface IAppRequestDL
    {

        Task<bool> InsertPostAppRequest(List<Winit.Modules.Syncing.Model.Interfaces.IAppRequest> appRequests);
        Task<int> UpdateAppRequest_IsNotificationReceived(List<string> uIDs, bool isNotificationReceived);
        Task<int> UpdateAppRequest_RequestPostedToAPITime(List<string> uIDs, DateTime dateTime);
        Task<IEnumerable<IAppRequest>> SelectAppRequestByUID(List<string> UIDs);
    }
}
