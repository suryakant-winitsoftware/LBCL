using Microsoft.AspNetCore.Components;
using Winit.Modules.SKU.Model.UIInterfaces;
using Winit.Shared.Models.Enums;
using Winit.Modules.SalesOrder.Model.UIInterfaces;
using Winit.Shared.Models.Common;
using WinIt.Pages.Base;
using Winit.Modules.SalesOrder.Model.Event;
using Winit.Shared.CommonUtilities.Common;
namespace WinIt.Pages.SalesOrder;

partial class CashSalesOrderGridView : BaseComponentBase
{
    private bool IsDDLOpen = false;
    private bool IsCloneItemOpen = false;
    private bool IsSelectAll = false;

    [Parameter]
    public List<Winit.Shared.Models.Common.ISelectionItem> UOMselectionItems { get; set; } = new List<ISelectionItem>();
    [Parameter]
    public List<Winit.Shared.Models.Common.ISelectionItem> UOMCloneselectionItems { get; set; } = new List<ISelectionItem>();
    [Parameter]
    public EventCallback<Winit.Modules.SalesOrder.Model.UIInterfaces.ISalesOrderItemView> OnCloneItem { get; set; }
    [Parameter]
    public EventCallback<Winit.Modules.SalesOrder.Model.UIInterfaces.ISalesOrderItemView> OnGetCloneSelectionItem { get; set; }
    [Parameter]
    public EventCallback<Winit.Modules.SalesOrder.Model.UIInterfaces.ISalesOrderItemView> OnGetDDLSelectionItem { get; set; }
    [Parameter]
    public EventCallback<Winit.Modules.SalesOrder.Model.UIInterfaces.ISalesOrderItemView> OnClonedItemDelete { get; set; }
    [Parameter]
    public EventCallback<SalesOrderItemEvent> OnQtyChanged { get; set; }
    [Parameter]
    public EventCallback<SalesOrderItemEvent> OnUnitPriceChanged { get; set; }
    [Parameter]
    public EventCallback<(Winit.Modules.SalesOrder.Model.UIInterfaces.ISalesOrderItemView, Winit.Shared.Models.Enums.StockType)> OnSKUTypeChanged { get; set; }
    [Parameter]
    public List<ISalesOrderItemView>? DisplayedSKUList { get; set; }
    [Parameter]
    public bool IsViewMode { get; set; } = true;
    [Parameter]
    public EventCallback<Winit.Shared.Models.Events.DropDownEvent> OnSelectUOM { get; set; }
    [Parameter]
    public bool IsBasePriceRequired { get; set; }
   
    private Winit.Modules.SalesOrder.Model.UIInterfaces.ISalesOrderItemView? selectedProduct;

    [Parameter]
    public bool DisableAfterFinalize { get; set; } = false;
    protected async override Task OnInitializedAsync()
    {
        LoadResources(null, _languageService.SelectedCulture);
        DisplayedSKUList ??= new();
        await Task.CompletedTask;
    }

    
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
                selectedProduct?.UsedUOMCodes.Add(selectionitem.Code);
                ISKUUOMView? newsKUUOM = selectedProduct?.AllowedUOMs.Find(uom => uom.Code == selectionitem.Code);
                if (newsKUUOM != null && selectedProduct != null)
                {
                    var cloneditem = selectedProduct.Clone(newsKUUOM, ItemState.Cloned, newsKUUOM.Code);
                    cloneditem.SelectedUOM = newsKUUOM;
                    OnCloneItem.InvokeAsync(cloneditem);
                }
            }
        }
        IsCloneItemOpen = false;
    }
    public void OnUOMSelection(Winit.Shared.Models.Events.DropDownEvent dropDownEvent)
    {
        OnSelectUOM.InvokeAsync(dropDownEvent);
        IsDDLOpen = false;
    }
    public void OnCloneBtnClick()
    {
        OnGetCloneSelectionItem.InvokeAsync(selectedProduct);
        IsCloneItemOpen = true;
    }
    public void OnDeleteBtnClick()
    {
        OnClonedItemDelete.InvokeAsync(selectedProduct);
    }
    public void HandleRowBtnClick(Winit.Modules.SalesOrder.Model.UIInterfaces.ISalesOrderItemView salesOrderItemView)
    {
        selectedProduct = salesOrderItemView;
        if (selectedProduct.ItemStatus == ItemState.Cloned)
        {
            OnDeleteBtnClick();
        }
        else
        {
            OnCloneBtnClick();
        }
    }
    public void OnPriceChange(string value)
    {
        if (selectedProduct is not null)
        {
            SalesOrderItemEvent salesOrderItemEvent = new SalesOrderItemEvent
            {
                SalesOrderItemView = selectedProduct,
                UnitPrice = decimal.Parse(value)
            };
            OnUnitPriceChanged.InvokeAsync(salesOrderItemEvent);
        }
    }
    public void OnQtyChange(string value)
    {
        decimal val=CommonFunctions.GetDecimalValue(value);
        if (selectedProduct is not null && val > 0)
        {
            SalesOrderItemEvent salesOrderItemEvent = new SalesOrderItemEvent
            {
                SalesOrderItemView = selectedProduct,
                Qty = val
            };
            OnQtyChanged.InvokeAsync(salesOrderItemEvent);
        }
        else
        {
            ShowErrorSnackBar(@Localizer["error"], @Localizer["quantity_should_not_be_negative_or_equal_to_zero.."]);
        }
    }

    public void HandleRowSelectAll(ChangeEventArgs changeEventArgs)
    {
        if (changeEventArgs.Value is bool isSelected)
        {
            IsSelectAll = isSelected;
            DisplayedSKUList?.ForEach(item => item.IsSelected = isSelected);
            StateHasChanged();
        }
    }

    private void Handle_CheckBox(ChangeEventArgs eventArgs, ISalesOrderItemView salesOrderItemView)
    {
        if (eventArgs.Value is bool checkbox) // Use pattern matching for type check
        {
            salesOrderItemView.IsSelected = checkbox;

            if (DisplayedSKUList != null)
            {
                int selectedItemsCount = DisplayedSKUList.Count(e => e.IsSelected); // Use Count() for clarity
                IsSelectAll = selectedItemsCount == DisplayedSKUList.Count; // Direct comparison
            }
        }
        else
        {
            Console.WriteLine(@Localizer["error_occured_while_handeling_the_check_btn"]);
        }
    }


}

