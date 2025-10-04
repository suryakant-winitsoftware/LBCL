using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Winit.Modules.Base.BL.Helper.Classes;
using Winit.Modules.Base.BL.Helper.Interfaces;
using Winit.Modules.SalesOrder.BL.UIInterfaces;
using Winit.Modules.SalesOrder.Model.UIClasses;
using Winit.Modules.SalesOrder.Model.UIInterfaces;
using Winit.Modules.Tax.BL.Interfaces;
using Winit.Modules.Tax.Model.Interfaces;
using Winit.Shared.CommonUtilities.Common;
using Winit.Shared.Models.Constants;

namespace Winit.Modules.SalesOrder.BL.UIClasses
{
    public class SalesOrderTaxCalculator : SalesOrderBase, ISalesOrderTaxCalculator
    {
        ITaxCalculator _taxCalculator;
        public SalesOrderTaxCalculator(ITaxCalculator taxCalculator)
        {
            _taxCalculator = taxCalculator;
        }
        public void SetSalesOrderViewModel(ISalesOrderViewModel salesOrderViewModel)
        {
            base.Initialize(salesOrderViewModel);
        }
        public async Task CalculateItemTaxes(ISalesOrderItemView salesOrderItemView)
        {
            if (salesOrderItemView.ApplicableTaxes == null || salesOrderItemView.ApplicableTaxes.Count == 0)
            {
                return;
            }
            List<IAppliedTax> appliedTaxes = await _taxCalculator.CalculateTaxes(salesOrderItemView.TotalAmount - salesOrderItemView.TotalDiscount,
                salesOrderItemView.ApplicableTaxes, _viewModel.TaxDictionary);

            if (appliedTaxes == null || appliedTaxes.Count == 0)
            {
                return;
            }
            salesOrderItemView.AppliedTaxes = appliedTaxes;
            salesOrderItemView.TotalLineTax = appliedTaxes.Sum(e => e.Amount);
            await Task.CompletedTask;
        }
        public async Task CalculateInvoiceTaxes()
        {
            if (_viewModel.InvoiceApplicableTaxes == null || _viewModel.InvoiceApplicableTaxes.Count == 0)
            {
                return;
            }
            List<IAppliedTax> appliedTaxes = await _taxCalculator.CalculateTaxes(_viewModel.TotalAmount - _viewModel.TotalDiscount,
                _viewModel.InvoiceApplicableTaxes, _viewModel.TaxDictionary);

            if (appliedTaxes == null || appliedTaxes.Count == 0)
            {
                return;
            }
            _viewModel.AppliedTaxes = appliedTaxes;
            _viewModel.TotalHeaderTax = appliedTaxes.Sum(e => e.Amount);
            await UpdateLineWiseProrataTax();
            await Task.CompletedTask;
        }
        public async Task UpdateLineWiseProrataTax()
        {
            if (_viewModel.TotalHeaderTax == 0)
            {
                return;
            }
            foreach (ISalesOrderItemView salesOrderItemView in _viewModel.SalesOrderItemViews.Where(e => e.IsCartItem))
            {
                // Distribute TotalCashDiscount in ratio of total amount
                if (_viewModel.TotalAmount == 0 || _viewModel.TotalHeaderTax == 0)
                {
                    salesOrderItemView.TotalHeaderTax = 0;
                }
                else
                {
                    salesOrderItemView.TotalHeaderTax = CommonFunctions.RoundForSystem(((salesOrderItemView.TotalAmount) / _viewModel.TotalAmount) * _viewModel.TotalHeaderTax);
                }
                await _viewModel.UpdateNetAmount(salesOrderItemView);
            }
            await Task.CompletedTask;
        }
        public List<string> GetApplicableTaxesByApplicableAt(Dictionary<string, ITax> taxDictionary, string applicableAt)
        {
            return _taxCalculator.GetApplicableTaxesByApplicableAt(taxDictionary, applicableAt);
        }
    }
}
