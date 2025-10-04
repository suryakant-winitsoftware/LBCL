using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Winit.Modules.SalesOrder.BL.UIInterfaces;
using Winit.Modules.SalesOrder.Model.Classes;
using Winit.Modules.SalesOrder.Model.UIInterfaces;
using Winit.Shared.CommonUtilities.Common;

namespace Winit.Modules.SalesOrder.BL.UIClasses
{
    public class OrderLevelCalculator : SalesOrderBase, UIInterfaces.IOrderLevelCalculator
    {
        public OrderLevelCalculator()
        {
        }
        public List<Model.UIInterfaces.ISalesOrderItemView> SalesOrderItemViews { get; set; }
        public void SetSalesOrderViewModel(ISalesOrderViewModel salesOrderViewModel)
        {
            base.Initialize(salesOrderViewModel);
        }
        protected override void UpdateInitialData()
        {
            SalesOrderItemViews = _viewModel.SalesOrderItemViews;//.Where(e => e.IsCartItem && e.Qty > 0).ToList();
        }
        public async Task ComputeOrderLevelTaxesAndOrderSummary()
        {
            CheckIfViewModelSet();
            UpdateHeader();
            // If Taxable customer then only calculate Tax
            if (_viewModel.SelectedStoreViewModel.IsTaxApplicable)
            {
                CalculateInvoiceTaxes();
            }
            await Task.CompletedTask;
        }
        private void UpdateHeaderAmount()
        {
            _viewModel.TotalAmount = CommonFunctions.RoundForSystem(SalesOrderItemViews.Sum(e => e.TotalAmount));
            _viewModel.TotalLineDiscount = CommonFunctions.RoundForSystem(SalesOrderItemViews.Sum(e => e.TotalDiscount));
            _viewModel.TotalHeaderDiscount = 0;
            _viewModel.TotalLineTax = CommonFunctions.RoundForSystem(SalesOrderItemViews.Sum(e => e.TotalLineTax));
            _viewModel.TotalHeaderTax = 0;
        }
        private void UpdateHeaderCount()
        {
            _viewModel.LineCount = SalesOrderItemViews.Where(e => e.IsCartItem).Count(); ;
            _viewModel.QtyCount = SalesOrderItemViews.Sum(e => e.Qty);
        }
        private void UpdateHeader()
        {
            UpdateHeaderAmount();
            UpdateHeaderCount();
        }
        private async Task CalculateInvoiceTaxes()
        {
            if(_viewModel._salesOrderTaxCalculator == null)
            {
                return;
            }
            await _viewModel._salesOrderTaxCalculator.CalculateInvoiceTaxes();
        }
    }
}
