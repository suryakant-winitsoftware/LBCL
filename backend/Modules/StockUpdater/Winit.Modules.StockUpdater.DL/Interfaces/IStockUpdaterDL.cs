using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Winit.Modules.StockUpdater.Model.Interfaces;

namespace Winit.Modules.StockUpdater.DL.Interfaces
{
    public interface IStockUpdaterDL
    {
        Task<int> UpdateStockAsync(List<IWHStockLedger> stockLedgers, IDbConnection? connection = null, IDbTransaction? transaction = null);
        Task<List<Winit.Modules.StockUpdater.Model.Interfaces.IWHStockSummary>?> GetWHStockSummary(string orgUID, string wareHouseUID);
    }
}
