using Azure;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.DependencyInjection;
using Nest;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Winit.Modules.Syncing.DL.Interfaces;
using Winit.Modules.Syncing.Model.Classes;
using Winit.Modules.Syncing.Model.Interfaces;
using Winit.Shared.CommonUtilities.Common;
using Winit.Shared.CommonUtilities.Extensions;
using Winit.Shared.Models.Common;

namespace Winit.Modules.Syncing.BL.Classes
{
    public class AppRequestBL :  Interfaces.IAppRequestBL
    {
        protected Winit.Modules.Syncing.DL.Interfaces.IAppRequestDL _appRequestDL;
        public AppRequestBL(Winit.Modules.Syncing.DL.Interfaces.IAppRequestDL appRequestDL)
        {
            _appRequestDL= appRequestDL;
        }
        public async Task<bool> PostAppRequest(List<IAppRequest> appRequests)
        {
            return await _appRequestDL.InsertPostAppRequest(appRequests);
        }
        public async Task<int> UpdateAppRequest_IsNotificationReceived(List<string> uIDs, bool isNotificationReceived)
        {
            return await _appRequestDL.UpdateAppRequest_IsNotificationReceived(uIDs, isNotificationReceived);
        }
        public async Task<int> UpdateAppRequest_RequestPostedToAPITime(List<string> uIDs, DateTime dateTime)
        {
            return await _appRequestDL.UpdateAppRequest_RequestPostedToAPITime(uIDs, dateTime);
        }
        public async Task<IEnumerable<IAppRequest>> SelectAppRequestByUID(List<string> UIDs)
        {
            return await _appRequestDL.SelectAppRequestByUID(UIDs);
        }
    }
}
