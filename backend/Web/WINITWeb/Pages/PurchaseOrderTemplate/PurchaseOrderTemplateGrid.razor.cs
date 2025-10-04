using Microsoft.AspNetCore.Components;
using Winit.Shared.Models.Common;

namespace WinIt.Pages.PurchaseOrderTemplate;

partial class PurchaseOrderTemplateGrid
{
    private bool IsDDLOpen = false;
    private bool IsCloneItemOpen = false;
    private bool IsSelectAll => DisplayedSKUList != null && DisplayedSKUList.Any() && DisplayedSKUList.Count(e => e.IsSelected) == DisplayedSKUList.Count;

    [Parameter]
    public List<Winit.Shared.Models.Common.ISelectionItem> UOMselectionItems { get; set; } = [];
    [Parameter]
    public List<Winit.Shared.Models.Common.ISelectionItem> UOMCloneselectionItems { get; set; } = [];
    [Parameter]
    public EventCallback<Winit.Modules.PurchaseOrder.BL.Interfaces.IPurchaseOrderTemplateItemView> OnCloneItem { get; set; }
    [Parameter]
    public EventCallback<Winit.Modules.PurchaseOrder.BL.Interfaces.IPurchaseOrderTemplateItemView> OnGetCloneSelectionItem { get; set; }
    [Parameter]
    public EventCallback<Winit.Modules.PurchaseOrder.BL.Interfaces.IPurchaseOrderTemplateItemView> OnGetDDLSelectionItem { get; set; }
    [Parameter]
    public EventCallback<Winit.Modules.PurchaseOrder.BL.Interfaces.IPurchaseOrderTemplateItemView> OnClonedItemDelete { get; set; }
    
    [Parameter]
    public EventCallback<(Winit.Modules.PurchaseOrder.BL.Interfaces.IPurchaseOrderTemplateItemView, Winit.Shared.Models.Enums.StockType)> OnSKUTypeChanged { get; set; }
    [Parameter]
    public List<Winit.Modules.PurchaseOrder.BL.Interfaces.IPurchaseOrderTemplateItemView>? DisplayedSKUList { get; set; }
    [Parameter]
    public bool IsViewMode { get; set; } = true;
    [Parameter]
    public EventCallback<Winit.Shared.Models.Events.DropDownEvent> OnSelectUOM { get; set; }
    [Parameter]
    public bool IsBasePriceRequired { get; set; }
    private Winit.Modules.PurchaseOrder.BL.Interfaces.IPurchaseOrderTemplateItemView? SelectedProduct;

    //protected override Task OnInitializedAsync()
    //{
    //    return base.OnInitializedAsync();
    //}
    private string CalculateTotalPrice(string orderQty, string price)
    {
        if (decimal.TryParse(orderQty, out decimal orderQtyValue) && decimal.TryParse(price, out decimal priceValue))
        {
            decimal totalPrice = orderQtyValue * priceValue;
            return totalPrice.ToString("0.00");
        }
        else
        {
            return string.Empty; // Handle invalid input as needed
        }
    }
    private void SelectedAddUOMType(Winit.Shared.Models.Events.DropDownEvent dropDownEvent)
    {
        if (dropDownEvent != null && dropDownEvent.SelectionItems != null && dropDownEvent.SelectionItems.Any())
        {
            ISelectionItem? selectionitem = dropDownEvent.SelectionItems.FirstOrDefault();
            if (selectionitem != null)
            {
                //_ = (SelectedProduct?.UsedUOMCodes.Add(selectionitem.Code));
                //ISKUUOMView? newsKUUOM = SelectedProduct?.AllowedUOMs.Find(uom => uom.Code == selectionitem.Code);
                //if (newsKUUOM != null && SelectedProduct != null)
                //{
                //    IPurchaseOrderTemplateItemView cloneditem = (IPurchaseOrderTemplateItemView)SelectedProduct.Clone(newsKUUOM, ItemState.Cloned, newsKUUOM.Code);
                //    cloneditem.SelectedUOM = newsKUUOM;
                //    _ = OnCloneItem.InvokeAsync(cloneditem);
                //    IsCloneItemOpen = false;
                //}
            }
        }
    }
    public void OnUOMSelection(Winit.Shared.Models.Events.DropDownEvent dropDownEvent)
    {
        _ = OnSelectUOM.InvokeAsync(dropDownEvent);
        IsDDLOpen = false;
    }
    public void OnCloneBtnClick()
    {
        _ = OnGetCloneSelectionItem.InvokeAsync(SelectedProduct);
        IsCloneItemOpen = true;
    }
    public void OnDeleteBtnClick()
    {
        _ = OnClonedItemDelete.InvokeAsync(SelectedProduct);
    }
    public void HandleRowBtnClick(Winit.Modules.PurchaseOrder.BL.Interfaces.IPurchaseOrderTemplateItemView PurchaseOrderItemView)
    {
        //SelectedProduct = PurchaseOrderItemView;
        //if (SelectedProduct.ItemStatus == ItemState.Cloned)
        //{
        //    OnDeleteBtnClick();
        //}
        //else
        //{
        //    OnCloneBtnClick();
        //}
    }
    
    public void OnQtyChange(decimal qty)
    {
        if (SelectedProduct is not null )
        {
            SelectedProduct.Qty = qty;
           
        }
        else if (SelectedProduct is not null && string.IsNullOrEmpty(qty.ToString()))
        {
            SelectedProduct.Qty = 0;
        }
    }

    public void HandleRowSelectAll(ChangeEventArgs changeEventArgs)
    {
        if (changeEventArgs.Value is bool isSelected)
        {
            DisplayedSKUList?.ForEach(item => item.IsSelected = isSelected);
            StateHasChanged();
        }
    }
}
