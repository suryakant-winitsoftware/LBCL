using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Winit.Modules.StockUpdater.Model.Interfaces;

namespace Winit.Modules.StockUpdater.BL.Interfaces
{
    public interface IStockUpdaterBL
    {
        Task<int> UpdateStockAsync(List<IWHStockLedger> stockLedgers);
        Task<List<Winit.Modules.StockUpdater.Model.Interfaces.IWHStockSummary>?> GetWHStockSummary(string orgUID, string wareHouseUID);
    }
}
