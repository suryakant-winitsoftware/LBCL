using Winit.Modules.PurchaseOrder.BL.Interfaces;
using Winit.Modules.PurchaseOrder.Model.Interfaces;
using Winit.Modules.Setting.BL.Interfaces;
using Winit.Modules.Tax.BL.Interfaces;
using Winit.Modules.Tax.Model.Interfaces;
using Winit.Shared.CommonUtilities.Common;

namespace Winit.Modules.PurchaseOrder.BL.Classes;

public class PurchaseOrderTaxCalculator : PurchaseOrderBase, IPurchaseOrderTaxCalculator
{
    ITaxCalculator _taxCalculator;
    private readonly IAppSetting _appSetting;

    public PurchaseOrderTaxCalculator(ITaxCalculator taxCalculator, IAppSetting appSetting)
    {
        _taxCalculator = taxCalculator;
        _appSetting = appSetting;
    }
    public void SetOrderViewModel(IPurchaseOrderViewModel purchaseOrderViewModel)
    {
        base.Initialize(purchaseOrderViewModel);
    }
    public async Task CalculateItemTaxes(IPurchaseOrderItemView purchaseOrderItemView)
    {
        if (purchaseOrderItemView.ApplicableTaxes == null || purchaseOrderItemView.ApplicableTaxes.Count == 0)
        {
            return;
        }
        List<IAppliedTax> appliedTaxes = await _taxCalculator.CalculateTaxes(purchaseOrderItemView.TotalAmount - purchaseOrderItemView.TotalDiscount,
            purchaseOrderItemView.ApplicableTaxes, _viewModel.TaxDictionary,_appSetting.IsPriceInclusiveVat);

        if (appliedTaxes == null || appliedTaxes.Count == 0)
        {
            return;
        }
        purchaseOrderItemView.AppliedTaxes = appliedTaxes;
        purchaseOrderItemView.LineTaxAmount = appliedTaxes.Sum(e => e.Amount);
        purchaseOrderItemView.TotalTaxAmount = purchaseOrderItemView.LineTaxAmount + purchaseOrderItemView.HeaderTaxAmount;
        purchaseOrderItemView.EffectiveUnitTax = CommonFunctions.RoundForSystem(purchaseOrderItemView.FinalQty == 0 ? 0 :
            purchaseOrderItemView.TotalTaxAmount / purchaseOrderItemView.FinalQty, _appSetting.RoundOffDecimal);
        await Task.CompletedTask;
        
    }
    public async Task CalculateInvoiceTaxes(IPurchaseOrderHeader? purchaseOrderHeader = null, List<IPurchaseOrderItemView>? purchaseOrderItemViews = null)
    {
        if (_viewModel.InvoiceApplicableTaxes == null || _viewModel.InvoiceApplicableTaxes.Count == 0)
        {
            return;
        }
        List<IAppliedTax> appliedTaxes;
        if (purchaseOrderHeader == null)
        {
            appliedTaxes = await _taxCalculator
            .CalculateTaxes(_viewModel.PurchaseOrderHeader.TotalAmount - _viewModel.PurchaseOrderHeader.TotalDiscount,
            _viewModel.InvoiceApplicableTaxes, _viewModel.TaxDictionary, _appSetting.IsPriceInclusiveVat);
        }
        else
        {
            appliedTaxes = await _taxCalculator
            .CalculateTaxes(purchaseOrderHeader.TotalAmount - purchaseOrderHeader.TotalDiscount,
            _viewModel.InvoiceApplicableTaxes, _viewModel.TaxDictionary, _appSetting.IsPriceInclusiveVat);
        }
        if (appliedTaxes == null || appliedTaxes.Count == 0)
        {
            return;
        }
        _viewModel.AppliedTaxes = appliedTaxes;
        if (purchaseOrderHeader == null)
        {
            _viewModel.PurchaseOrderHeader.HeaderTaxAmount = appliedTaxes.Sum(e => e.Amount);
        }
        else
        {
            purchaseOrderHeader.HeaderTaxAmount = appliedTaxes.Sum(e => e.Amount);
        }
        UpdateLineWiseProrataTax(purchaseOrderHeader, purchaseOrderItemViews);
        await Task.CompletedTask;
    }
    public void UpdateLineWiseProrataTax(IPurchaseOrderHeader purchaseOrderHeader, List<IPurchaseOrderItemView> purchaseOrderItemViews)
    {
        if (purchaseOrderHeader == null)
        {
            if (_viewModel.PurchaseOrderHeader.HeaderTaxAmount == 0)
            {
                return;
            }
            foreach (IPurchaseOrderItemView purchaseOrderItemView in _viewModel.PurchaseOrderItemViews.Where(e => e.IsCartItem))
            {
                // Distribute TotalCashDiscount in ratio of total amount
                if (_viewModel.PurchaseOrderHeader.TotalAmount == 0 || _viewModel.PurchaseOrderHeader.HeaderTaxAmount == 0)
                {
                    purchaseOrderItemView.HeaderTaxAmount = 0;
                }
                else
                {
                    purchaseOrderItemView.HeaderTaxAmount = CommonFunctions.RoundForSystem(((purchaseOrderItemView.TotalAmount) / _viewModel.PurchaseOrderHeader.TotalAmount) * _viewModel.PurchaseOrderHeader.HeaderTaxAmount);
                }
                _viewModel.UpdateNetAmount(purchaseOrderItemView);
            }
        }
        else
        {
            if (purchaseOrderHeader.HeaderTaxAmount == 0)
            {
                return;
            }
            foreach (IPurchaseOrderItemView purchaseOrderItemView in purchaseOrderItemViews.Where(e => e.IsCartItem))
            {
                // Distribute TotalCashDiscount in ratio of total amount
                if (purchaseOrderHeader.TotalAmount == 0 || purchaseOrderHeader.HeaderTaxAmount == 0)
                {
                    purchaseOrderItemView.HeaderTaxAmount = 0;
                }
                else
                {
                    purchaseOrderItemView.HeaderTaxAmount = CommonFunctions.RoundForSystem(((purchaseOrderItemView.TotalAmount) / purchaseOrderHeader.TotalAmount) * purchaseOrderHeader.HeaderTaxAmount);
                }
                _viewModel.UpdateNetAmount(purchaseOrderItemView);
            }
        }
    }
    public List<string> GetApplicableTaxesByApplicableAt(Dictionary<string, ITax> taxDictionary, string applicableAt)
    {
        return _taxCalculator.GetApplicableTaxesByApplicableAt(taxDictionary, applicableAt);
    }
}
