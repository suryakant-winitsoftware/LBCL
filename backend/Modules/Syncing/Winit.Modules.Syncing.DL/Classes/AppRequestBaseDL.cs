using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Winit.Modules.Syncing.Model.Interfaces;

namespace Winit.Modules.Syncing.DL.Classes
{
    public abstract class AppRequestBaseDL : Base.DL.DBManager.SqliteDBManager
    {
        public AppRequestBaseDL(IServiceProvider serviceProvider) : base(serviceProvider)
        {

        }
        public async Task<bool> InsertPostAppRequest(List<IAppRequest> appRequests)
        {
            if (appRequests == null || appRequests.Count == 0)
            {
                return true;
            }
            List<string> uidList = appRequests.Select(ar => ar.UID).ToList();
            try
            {
                List<string> existingUIDs = await CheckAppRequestExistsByUID(uidList);
                List<IAppRequest> appRequestsNew = appRequests.Where(e => !existingUIDs.Contains(e.UID)).ToList();
                if (appRequestsNew == null || appRequestsNew.Count == 0)
                {
                    return true;
                }
                int result = await CreateAppRequest(appRequestsNew);
                return true;
            }
            catch (Exception)
            {
                throw;
            }
        }
        protected abstract Task<List<string>> CheckAppRequestExistsByUID(List<string> UIDs);
        protected abstract Task<int> CreateAppRequest(List<IAppRequest> appRequests);
    }
}
