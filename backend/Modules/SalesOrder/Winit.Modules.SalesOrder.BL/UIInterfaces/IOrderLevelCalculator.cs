using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Winit.Modules.SalesOrder.BL.UIInterfaces
{
    public interface IOrderLevelCalculator
    {
        List<Model.UIInterfaces.ISalesOrderItemView> SalesOrderItemViews { get; set; }
        void SetSalesOrderViewModel(ISalesOrderViewModel salesOrderViewModel);
        Task ComputeOrderLevelTaxesAndOrderSummary();
    }
}
