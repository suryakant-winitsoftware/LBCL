using Microsoft.Data.SqlClient;
using Nest;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Winit.Modules.Syncing.Model.Classes;
using Winit.Modules.Syncing.Model.Interfaces;

namespace Winit.Modules.Syncing.BL.Interfaces
{
    public interface IAppRequestBL
    {
        Task<bool> PostAppRequest(List<IAppRequest> appRequests);
        Task<int> UpdateAppRequest_IsNotificationReceived(List<string> uIDs, bool isNotificationReceived);
        Task<int> UpdateAppRequest_RequestPostedToAPITime(List<string> uIDs, DateTime dateTime);
        Task<IEnumerable<IAppRequest>> SelectAppRequestByUID(List<string> UIDs);
    }
}
