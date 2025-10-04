using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Winit.Modules.StockUpdater.BL.Interfaces;
using Winit.Modules.StockUpdater.Model.Interfaces;

namespace Winit.Modules.StockUpdater.BL.Classes
{
    public class StockUpdaterBL: IStockUpdaterBL
    {
        protected readonly DL.Interfaces.IStockUpdaterDL _stockUpdaterDL = null;
        IServiceProvider _serviceProvider = null;
        public StockUpdaterBL(DL.Interfaces.IStockUpdaterDL stockUpdaterDL, IServiceProvider serviceProvider)
        {
            _stockUpdaterDL = stockUpdaterDL;
            _serviceProvider = serviceProvider;
        }
        public async Task<int> UpdateStockAsync(List<IWHStockLedger> stockLedgers)
        {
            return await _stockUpdaterDL.UpdateStockAsync(stockLedgers);
        }
        public async Task<List<Winit.Modules.StockUpdater.Model.Interfaces.IWHStockSummary>?> GetWHStockSummary(string orgUID, string wareHouseUID)
        {
            return await _stockUpdaterDL.GetWHStockSummary(orgUID, wareHouseUID);
        }
    }
}
