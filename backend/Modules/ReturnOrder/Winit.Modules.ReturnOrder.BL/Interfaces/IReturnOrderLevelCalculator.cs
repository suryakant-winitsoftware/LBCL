using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Winit.Modules.ReturnOrder.BL.Interfaces
{
    public interface IReturnOrderLevelCalculator
    {
        IReturnOrderViewModel _returnOrderViewModel { get; set; }
        Task ComputeOrderLevelTaxesAndOrderSummary();
    }
}
