using Winit.Modules.PurchaseOrder.BL.Interfaces;
using Winit.Shared.CommonUtilities.Common;

namespace Winit.Modules.PurchaseOrder.BL.Classes; 

public class PurchaseOrderLevelCalculator:PurchaseOrderBase,IPurchaseOrderLevelCalculator
{
    public List<IPurchaseOrderItemView> PurchaseOrderItemViews { get; set; }
    public void SetOrderViewModel(IPurchaseOrderViewModel purchaseOrderViewModel)
    {
        base.Initialize(purchaseOrderViewModel);
    }
    protected override void UpdateInitialData()
    {
        PurchaseOrderItemViews = _viewModel.PurchaseOrderItemViews;//.Where(e => e.IsCartItem && e.Qty > 0).ToList();
    }
    public async Task ComputeOrderLevelTaxesAndOrderSummary()
    {
        CheckIfViewModelSet();
        UpdateHeader();
        // If Taxable customer then only calculate Tax
        if (_viewModel.SelectedStoreMaster!.Store!.IsTaxApplicable)
        {
            CalculateInvoiceTaxes();
        }
        _viewModel.PurchaseOrderHeader.TotalTaxAmount = _viewModel.PurchaseOrderHeader.HeaderTaxAmount + _viewModel.PurchaseOrderHeader.LineTaxAmount;
        await Task.CompletedTask;
    }
    private void UpdateHeaderAmount()
    {
        _viewModel.PurchaseOrderHeader.TotalAmount = CommonFunctions.RoundForSystem(PurchaseOrderItemViews.Sum(e => e.TotalAmount));
        _viewModel.PurchaseOrderHeader.LineDiscount = CommonFunctions.RoundForSystem(PurchaseOrderItemViews.Sum(e => e.TotalDiscount));
        _viewModel.PurchaseOrderHeader.TotalDiscount = CommonFunctions.RoundForSystem(PurchaseOrderItemViews.Sum(e => e.TotalDiscount));
        _viewModel.PurchaseOrderHeader.LineTaxAmount = CommonFunctions.RoundForSystem(PurchaseOrderItemViews.Sum(e => e.LineTaxAmount));
        _viewModel.PurchaseOrderHeader.TotalTaxAmount = 0;
    }
    private void UpdateHeaderCount()
    {
        _viewModel.PurchaseOrderHeader.LineCount = PurchaseOrderItemViews.Where(e => e.IsCartItem).Count(); ;
        _viewModel.PurchaseOrderHeader.QtyCount = PurchaseOrderItemViews.Sum(e => e.FinalQty);
    }
    private void UpdateHeader()
    {
        UpdateHeaderAmount();
        UpdateHeaderCount();
    }
    private void CalculateInvoiceTaxes()
    {
        if (_viewModel._purchaseOrderTaxCalculator == null)
        {
            return;
        }
        _viewModel._purchaseOrderTaxCalculator.CalculateInvoiceTaxes();
    }
}
