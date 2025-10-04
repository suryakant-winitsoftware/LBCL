using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Winit.Modules.JourneyPlan.Model.Interfaces;
using Winit.Shared.Models.Common;
using Winit.Shared.Models.Enums;

namespace Winit.Modules.JourneyPlan.DL.Interfaces;

public interface IStoreHistoryDL
{
   
    Task<Winit.Modules.JourneyPlan.Model.Interfaces.IStoreHistory?> SelectStoreHistory_ByUID(string UID,
       IDbConnection? connection = null, IDbTransaction? transaction = null);
    Task<int> CUDStoreHistory(Winit.Modules.JourneyPlan.Model.Interfaces.IStoreHistory storeHistory,
       IDbConnection? connection = null, IDbTransaction? transaction = null);
    Task<int> CreateStoreHistory(Winit.Modules.JourneyPlan.Model.Interfaces.IStoreHistory storeHistory, IDbConnection? connection = null, IDbTransaction? transaction = null);
    Task<int> UpdateStoreHistory(Winit.Modules.JourneyPlan.Model.Interfaces.IStoreHistory storeHistory, IDbConnection? connection = null, IDbTransaction? transaction = null);


}