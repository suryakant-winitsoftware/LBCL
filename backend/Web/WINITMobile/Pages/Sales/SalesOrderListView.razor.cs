using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web.Virtualization;
using Microsoft.JSInterop;
using Winit.Modules.Promotion.Model.Classes;
using Winit.Modules.SalesOrder.Model.UIInterfaces;
using Winit.Shared.Models.Common;
using Winit.Shared.Models.Events;
using Winit.UIComponents.Common.Common;
using Winit.UIComponents.Common.Services;

namespace WINITMobile.Pages.Sales;

partial class SalesOrderListView
{
    // for promo
    [Parameter]
    public EventCallback<ISalesOrderItemView> OnPromotionClick { get; set; }
    [Parameter]
    public List<Winit.Modules.SalesOrder.Model.UIInterfaces.ISalesOrderItemView> SalesOrderItemViewList { get; set; }
    [Parameter]
    public bool IsGridView { get; set; }
    [Parameter]
    public EventCallback<QtyChangeEvent> OnQtyChange { get; set; }
    [Parameter]
    public EventCallback<ISalesOrderItemView> OnDeleteClonedProduct { get; set; }
    [Parameter]
    public Func<Winit.Modules.SalesOrder.Model.UIInterfaces.ISalesOrderItemView, List<ISelectionItem>> OnUOMDDClick { get; set; }
    [Parameter]
    public Func<Winit.Modules.SalesOrder.Model.UIInterfaces.ISalesOrderItemView, List<ISelectionItem>> OnCloneBtnClick { get; set; }
    [Parameter]
    public EventCallback<DropDownEvent> DropDownSelection_OnSingleSelect { get; set; }
    [Parameter]
    public EventCallback<DropDownEvent> DropDownClone_OnSingleSelect { get; set; }
    [Parameter]
    public List<Winit.Shared.Models.Common.ISelectionItem> LeftScrollItems { get; set; }
    [Parameter]
    public EventCallback<Winit.Shared.Models.Common.ISelectionItem> OnLeftScrollItmSelect { get; set; }
    [Parameter]
    public EventCallback<(Winit.Modules.SalesOrder.Model.UIInterfaces.ISalesOrderItemView item, decimal price)> OnUnitPriceChange { get; set; }
    [Parameter]
    public DmsPromotion SelectedPromotion { get; set; }
    [Parameter]
    public EventCallback OnPromotionRemoveClick { get; set; }
    [Parameter]
    public Func<ISalesOrderItemView, List<ISelectionItem>> OnGetPromotionsForItem { get; set; }
    [Parameter]
    public Func<QtyChangeEvent, Task<bool>> OnPreValidateQtyChange { get; set; }
    private Virtualize<ISalesOrderItemView>? virtualizeRef;
    private bool IsModalOpen { get; set; } = false;
    private bool IsEditPriceOpen = false;
    public string Initialpromo { get; set; } = "";
    private ISalesOrderItemView SelectedProduct;
    private List<ISelectionItem> ItemPromotions = new List<ISelectionItem>();
    public bool IsSingleUOM { get; set; } = false;
    private async ValueTask<ItemsProviderResult<ISalesOrderItemView>> LoadItems(ItemsProviderRequest request)
    {
        await Task.Yield();
        var numItems = Math.Min(request.Count, SalesOrderItemViewList.Count - request.StartIndex);
        var items = SalesOrderItemViewList.Skip(request.StartIndex).Take(numItems).ToList();
        return new ItemsProviderResult<ISalesOrderItemView>(items, SalesOrderItemViewList.Count);

    }
    public async Task RefreshVirtualizedItems()
    {
        if (virtualizeRef != null)
        {
            await virtualizeRef.RefreshDataAsync();
        }
    }
    private void ShowProductDetails(ISalesOrderItemView product)
    {
        if (product.ItemType == Winit.Shared.Models.Enums.ItemType.FOC)
        {
            return;
        }
        ItemPromotions.Clear();
        var promos = OnGetPromotionsForItem.Invoke(product);
        if (promos is not null) ItemPromotions.AddRange(promos);
        SelectedProduct = product;
        IsModalOpen = true;
    }

    public void SelectedValueChanged(DropDownEvent dropDownEvent)
    {
        InvokeAsync(() => DropDownSelection_OnSingleSelect.InvokeAsync(dropDownEvent));
    }

    public void CreateCloneForSkU(DropDownEvent dropDownEvent)
    {
        InvokeAsync(() => DropDownClone_OnSingleSelect.InvokeAsync(dropDownEvent));
    }

    private async Task OnUOMButtonClick(Winit.Modules.SalesOrder.Model.UIInterfaces.ISalesOrderItemView salesOrderItemView)
    {
        if (salesOrderItemView.ItemStatus.ToString() == "Cloned")
        {
        }
        else
        {
            SelectedProduct = salesOrderItemView;
            var uomSelectionItems = OnUOMDDClick.Invoke(salesOrderItemView);
            //IsOpenUOM = true;
            await _dropdownService.ShowMobilePopUpDropDown(new DropDownOptions
            {
                DataSource = uomSelectionItems,
                UniqueUID = SelectedProduct.UID,
                OnSelect = (eventArgs) =>
                    Task.Run(() => SelectedValueChanged(eventArgs)),
                OkBtnTxt = @Localizer["proceed"],
                Title = @Localizer["uom"]
            });
        }
    }

    private async Task OnCloneUOMButtonClick(Winit.Modules.SalesOrder.Model.UIInterfaces.ISalesOrderItemView salesOrderItemView)
    {
        SelectedProduct = salesOrderItemView;
        var cloneSelectionItems = OnCloneBtnClick.Invoke(salesOrderItemView);
        if (SelectedProduct.SelectedUOM == null)
        {
            Console.WriteLine(@Localizer["select_uom_first"]);
        }
        else
        {
            if (salesOrderItemView.ItemStatus.ToString() == "Cloned")
            {
                if (await _alertService.ShowConfirmationReturnType(@Localizer["delete"], @Localizer["are_you_sure_you_want_to_delete_this_item?"]))
                {
                    await OnDeleteClonedProduct.InvokeAsync(SelectedProduct);
                }
            }
            else
            {
                await _dropdownService.ShowMobilePopUpDropDown(new DropDownOptions
                {
                    DataSource = cloneSelectionItems,
                    UniqueUID = SelectedProduct.UID,
                    OnSelect = async (eventArgs) =>
                    {
                        await Task.Run(() => CreateCloneForSkU(eventArgs));
                    },
                    OkBtnTxt = @Localizer["proceed"],
                    Title = @Localizer["uom"]
                });
            }
        }
    }
    private async Task AddButtonOnQtyChange(QtyChangeEvent qtyChangeEvent)
    {
        await OnQtyChange.InvokeAsync(qtyChangeEvent);
    }
    private async Task<bool> PreValidateQtyChange(QtyChangeEvent qtyChangeEvent)
    {
        if (OnPreValidateQtyChange != null)
        {
            return await OnPreValidateQtyChange.Invoke(qtyChangeEvent);
        }
        return true;
    }
    private async Task UpdatePriceValue(decimal newPrice)
    {
        await OnUnitPriceChange.InvokeAsync((SelectedProduct, newPrice));
        IsEditPriceOpen = false;
    }
    private void ShowPromotionData(ISalesOrderItemView salesOrderItemView)
    {
        // Call the OnPromotionClick event with applicable promotions
        OnPromotionClick.InvokeAsync(salesOrderItemView);
    }
    private async Task OnLeftItemSelect(ISelectionItem selectionItem)
    {
        await OnLeftScrollItmSelect.InvokeAsync(selectionItem);
        StateHasChanged();
    }
    //Selva
    //protected override async void OnInitialized()
    //{
    //    LoadResources(null, _languageService.SelectedCulture);

    //}

}