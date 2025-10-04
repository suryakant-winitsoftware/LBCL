using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Winit.Modules.Store.Model.Interfaces;

namespace Winit.Modules.Store.DL.Interfaces;

public interface IStoreHistoryDL
{
    Task<IStoreHistory?> GetStoreHistoryByRouteUIDVisitDateAndStoreUID(string routeUID, string visitDate, string storeUID);
}
