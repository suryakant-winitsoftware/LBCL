using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Winit.Modules.SalesOrder.BL.UIInterfaces;
using Winit.Modules.SalesOrder.Model.Classes;
using Winit.Modules.SalesOrder.Model.UIClasses;
using Winit.Modules.SalesOrder.Model.UIInterfaces;
using Winit.Shared.CommonUtilities.Common;

namespace Winit.Modules.SalesOrder.BL.UIClasses
{
    public class CashDiscountCalculator : SalesOrderBase, ICashDiscountCalculator
    {
        public CashDiscountCalculator()
        {
        }
        public void SetSalesOrderViewModel(ISalesOrderViewModel salesOrderViewModel)
        {
            base.Initialize(salesOrderViewModel);
        }
        public async Task CalculateCashDiscount(decimal discountValue)
        {
            CheckIfViewModelSet();
            _viewModel.TotalCashDiscount = discountValue;
            decimal totalAmount = _viewModel.TotalAmount;
            decimal totalCashDiscount = _viewModel.TotalCashDiscount;

            foreach (ISalesOrderItemView salesOrderItemView in _viewModel.SelectedSalesOrderItemViews)
            {
                // Distribute TotalCashDiscount in ratio of total amount
                if (totalAmount == 0 || totalCashDiscount == 0)
                {
                    salesOrderItemView.TotalCashDiscount = 0;
                }
                else
                {
                    salesOrderItemView.TotalCashDiscount = CommonFunctions.RoundForSystem(((salesOrderItemView.TotalAmount) / totalAmount) * totalCashDiscount);
                }
            }
            await Task.FromResult(true);
        }
    }
}
