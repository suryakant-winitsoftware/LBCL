using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Winit.Modules.Chart.DL.Interfaces
{
    public interface IChartDL
    {
        Task<Dictionary<string, object>> GetPurchaseOrderAndTallyDashBoard();
    }
}
