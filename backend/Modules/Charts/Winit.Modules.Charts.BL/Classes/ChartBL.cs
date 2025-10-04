using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Winit.Modules.Chart.BL.Interfaces;
using Winit.Modules.Chart.DL.Interfaces;
namespace Winit.Modules.Chart.BL.Classes
{
    public class ChartBL: IChartBL
    {
        protected readonly IChartDL _chartDL;
        public ChartBL(IChartDL chartDL)
        {
            _chartDL = chartDL;   
        }
       public async Task<Dictionary<string, object>> GetPurchaseOrderAndTallyDashBoard()
        {
            return await _chartDL.GetPurchaseOrderAndTallyDashBoard();
        }
     
    }
}
