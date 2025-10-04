using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Winit.Modules.SalesOrder.BL.UIInterfaces
{
    public interface ICashDiscountCalculator
    {
        Task CalculateCashDiscount(decimal discountValue);
        void SetSalesOrderViewModel(ISalesOrderViewModel salesOrderViewModel);
    }
}
