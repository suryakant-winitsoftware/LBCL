using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Winit.Modules.Base.BL.Helper.Interfaces;
using Winit.Modules.SalesOrder.Model.UIInterfaces;
using Winit.Modules.Tax.Model.Interfaces;

namespace Winit.Modules.SalesOrder.BL.UIInterfaces
{
    public interface ISalesOrderTaxCalculator 
    {
        void SetSalesOrderViewModel(ISalesOrderViewModel salesOrderViewModel);
        Task CalculateItemTaxes(ISalesOrderItemView salesOrderItemView);
        Task CalculateInvoiceTaxes();
        List<string> GetApplicableTaxesByApplicableAt(Dictionary<string, ITax> taxDictionary, string applicableAt);
    }
}
