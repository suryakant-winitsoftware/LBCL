using Microsoft.AspNetCore.Components;
using Winit.Modules.PurchaseOrder.BL.Events;
using Winit.Modules.PurchaseOrder.BL.Interfaces;
using Winit.Modules.PurchaseOrder.Model.Interfaces;
using Winit.Modules.SalesOrder.Model.Event;
using Winit.Shared.CommonUtilities.Common;
using Winit.Shared.Models.Common;
using WinIt.Pages.Base;

namespace WinIt.Pages;

partial class PurchaseOrderGrid : BaseComponentBase
{
    private bool IsDDLOpen = false;
    private bool IsCloneItemOpen = false;

    private bool IsSelectAll => DisplayedSKUList != null && DisplayedSKUList.Any() &&
        DisplayedSKUList.Count(e => e.IsSelected) == DisplayedSKUList.Count;

    [Parameter] public List<Winit.Shared.Models.Common.ISelectionItem> UOMselectionItems { get; set; } = [];
    [Parameter] public List<Winit.Shared.Models.Common.ISelectionItem> UOMCloneselectionItems { get; set; } = [];

    [Parameter]
    public EventCallback<Winit.Modules.PurchaseOrder.BL.Interfaces.IPurchaseOrderItemView> OnCloneItem { get; set; }

    [Parameter]
    public EventCallback<Winit.Modules.PurchaseOrder.BL.Interfaces.IPurchaseOrderItemView> OnGetCloneSelectionItem
    {
        get;
        set;
    }

    [Parameter]
    public EventCallback<Winit.Modules.PurchaseOrder.BL.Interfaces.IPurchaseOrderItemView> OnGetDDLSelectionItem
    {
        get;
        set;
    }

    [Parameter]
    public EventCallback<Winit.Modules.PurchaseOrder.BL.Interfaces.IPurchaseOrderItemView> OnClonedItemDelete
    {
        get;
        set;
    }

    [Parameter]
    public bool DisplayBilledAndCancelledQty { get; set; }

    [Parameter] public EventCallback<PurchaseOrderItemEvent> OnQtyChanged { get; set; }
    [Parameter] public EventCallback<SalesOrderItemEvent> OnUnitPriceChanged { get; set; }
    [Parameter] public bool IsNewOrder { get; set; }

    [Parameter]
    public EventCallback<(Winit.Modules.PurchaseOrder.BL.Interfaces.IPurchaseOrderItemView,
        Winit.Shared.Models.Enums.StockType)> OnSKUTypeChanged { get; set; }

    [Parameter] public List<IPurchaseOrderItemView>? DisplayedSKUList { get; set; }
    [Parameter] public bool IsViewMode { get; set; } = true;
    [Parameter] public EventCallback<Winit.Shared.Models.Events.DropDownEvent> OnSelectUOM { get; set; }
    [Parameter] public bool IsBasePriceRequired { get; set; }
    [Parameter] public bool DisableQtyField { get; set; }
    private Winit.Modules.PurchaseOrder.BL.Interfaces.IPurchaseOrderItemView? SelectedProduct;
    [Parameter]
    public EventCallback<IPurchaseOrderItemView> OnProvisionChangeClick { get; set; }

    [Parameter] public bool IsForApproval { get; set; }

    private int? CountforTemp { get; set; }

    public bool ShowProvisioningPopUp { get; set; }

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
            return string.Empty;// Handle invalid input as needed
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
                //    IPurchaseOrderItemView cloneditem = (IPurchaseOrderItemView)SelectedProduct.Clone(newsKUUOM, ItemState.Cloned, newsKUUOM.Code);
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

    public void HandleRowBtnClick(
        Winit.Modules.PurchaseOrder.BL.Interfaces.IPurchaseOrderItemView PurchaseOrderItemView)
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

    public void OnPriceChange(string value)
    {
        if (SelectedProduct is not null)
        {
            SalesOrderItemEvent SalesOrderItemEvent = new()
            {
                //PurchaseOrderItemView = SelectedProduct,
                UnitPrice = decimal.Parse(value)
            };
            _ = OnUnitPriceChanged.InvokeAsync(SalesOrderItemEvent);
        }
    }

    // public void OnQtyChange(string value)
    // {
    //     if (SelectedProduct is not null && decimal.TryParse(value, out decimal qty))
    //     {
    //         if (IsForApproval && qty > SelectedProduct.RequestedQty) return;
    //         PurchaseOrderItemEvent SalesOrderItemEvent = new PurchaseOrderItemEvent
    //         {
    //             PurchaseOrderItemView = SelectedProduct,
    //             Qty = CommonFunctions.RoundForSystem(qty, 0)
    //         };
    //         _ = OnQtyChanged.InvokeAsync(SalesOrderItemEvent);
    //     }
    //     else if (SelectedProduct is not null && string.IsNullOrEmpty(value))
    //     {
    //         PurchaseOrderItemEvent SalesOrderItemEvent = new PurchaseOrderItemEvent
    //         {
    //             PurchaseOrderItemView = SelectedProduct,
    //             Qty = 0
    //         };
    //         _ = OnQtyChanged.InvokeAsync(SalesOrderItemEvent);
    //     }
    // }
    public void OnQtyChange(decimal qty)
    {
        // if (SelectedProduct is not null && decimal.TryParse(value, out decimal qty))
        // {
        if (IsForApproval && qty > SelectedProduct.RequestedQty) return;
        PurchaseOrderItemEvent SalesOrderItemEvent = new PurchaseOrderItemEvent
        {
            PurchaseOrderItemView = SelectedProduct, Qty = CommonFunctions.RoundForSystem(qty, 0)
        };
        _ = OnQtyChanged.InvokeAsync(SalesOrderItemEvent);
        // }
        // else if (SelectedProduct is not null && string.IsNullOrEmpty(value))
        // {
        //     PurchaseOrderItemEvent SalesOrderItemEvent = new PurchaseOrderItemEvent
        //     {
        //         PurchaseOrderItemView = SelectedProduct,
        //         Qty = 0
        //     };
        //     _ = OnQtyChanged.InvokeAsync(SalesOrderItemEvent);
        // }
    }
    public void HandleRowSelectAll(ChangeEventArgs changeEventArgs)
    {
        if (changeEventArgs.Value is bool isSelected)
        {
            DisplayedSKUList?.ForEach(item => item.IsSelected = isSelected);
            StateHasChanged();
        }
    }

    private void Handle_CheckBox(ChangeEventArgs eventArgs, IPurchaseOrderItemView PurchaseOrderItemView)
    {
        if (eventArgs.Value is bool checkbox)// Use pattern matching for type check
        {
            PurchaseOrderItemView.IsSelected = checkbox;
        }
        else
        {
            //Console.WriteLine(@Localizer["error_occured_while_handeling_the_check_btn"]);
        }
    }
    private void OnProvisionPopUpUpdateClick(List<IPurchaseOrderLineProvision> purchaseOrderLineProvisions)
    {
        //SelectedProduct.PurchaseOrderLineProvisions.Clear();
        //SelectedProduct.PurchaseOrderLineProvisions.AddRange(purchaseOrderLineProvisions);
        OnQtyChange(SelectedProduct.FinalQty);
        ShowProvisioningPopUp = false;
        ShowSuccessSnackBar("Success", "Provision updated");
    }
}
