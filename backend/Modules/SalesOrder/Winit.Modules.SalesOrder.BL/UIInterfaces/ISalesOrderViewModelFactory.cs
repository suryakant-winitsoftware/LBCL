using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Winit.Modules.SalesOrder.BL.UIInterfaces
{
    public interface ISalesOrderViewModelFactory
    {
        public ISalesOrderViewModel CreateSalesOrderViewModel(string SalesType);
    }
}
