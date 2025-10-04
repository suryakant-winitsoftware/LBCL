using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;

using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Json;
using Winit.Modules.SKU.Model.UIInterfaces;
using Winit.Shared.Models.Enums;
using Winit.Modules.ReturnOrder.Model.Interfaces;
using Winit.Modules.ReturnOrder.Model.Classes;
using System.Net.Http;
using System.Text;
using Winit.Modules.Common.Model.Interfaces;
using Winit.Shared.CommonUtilities.Extensions;
using Winit.Modules.SKU.Model.Classes;
using System.ComponentModel.DataAnnotations;
using Winit.Shared.Models.Common;
using Winit.Modules.SKU.Model.Interfaces;
using Newtonsoft.Json.Linq;
using Winit.Modules.Common.BL;
using Winit.Modules.ReturnOrder.BL.Classes;
using System.Reflection;
using WinIt.Pages.Base;
using System.Globalization;
using System.Resources;

using Winit.UIComponents.Common.Language;

namespace WinIt.Pages.ReturnOrder;

public partial class CreateReturnOrderGridview : BaseComponentBase
{
    private bool _isCloneItemOpen = false;

    [Parameter]
    public List<Winit.Shared.Models.Common.ISelectionItem> UOMCloneselectionItems { get; set; }
    [Parameter]
    public EventCallback<Winit.Modules.ReturnOrder.Model.Interfaces.IReturnOrderItemView> OnCloneItem { get; set; }
    [Parameter]
    public EventCallback<Winit.Modules.ReturnOrder.Model.Interfaces.IReturnOrderItemView> OnGetCloneSelectionItem { get; set; }
    [Parameter]
    public Func<Winit.Modules.ReturnOrder.Model.Interfaces.IReturnOrderItemView, List<ISelectionItem>> OnGetDDLSelectionItem { get; set; }
    [Parameter]
    public EventCallback<Winit.Modules.ReturnOrder.Model.Interfaces.IReturnOrderItemView> OnClonedItemDelete { get; set; }
    [Parameter]
    public EventCallback<Winit.Modules.ReturnOrder.Model.Interfaces.IReturnOrderItemView> OnQtyChanged { get; set; }
    [Parameter]
    public EventCallback<(Winit.Modules.ReturnOrder.Model.Interfaces.IReturnOrderItemView, Winit.Shared.Models.Enums.StockType)> OnSKUTypeChanged { get; set; }
    [Parameter]
    public List<IReturnOrderItemView> DisplayedSKUList { get; set; }
    [Parameter]
    public bool IsViewMode { get; set; } = true;
    [Parameter]
    public bool IsWithInvoice { get; set; }
    [Parameter]
    public EventCallback<Winit.Shared.Models.Events.DropDownEvent> OnSelectUOM { get; set; }
    private Winit.Modules.ReturnOrder.Model.Interfaces.IReturnOrderItemView selectedProduct = null;
    private List<ISelectionItem> ReasonTypeSelectionItems { get; set; }
    protected async override Task OnInitializedAsync()
    {
        LoadResources(null, _languageService.SelectedCulture);
        ReasonTypeSelectionItems = new List<ISelectionItem>
        {
            new SelectionItem
            {
                UID = "Salable",
                Label = StockType.Salable.ToString(),
                Code = StockType.Salable.ToString(),
            },
            new SelectionItem
            {
                UID = "NonSalable",
                Label = StockType.NonSalable.ToString(),
                Code = StockType.NonSalable.ToString(),
            }
        };
        DisplayedSKUList ??= new();
    }

    private async Task OnReasonTypeSelect(Winit.Shared.Models.Events.DropDownEvent dropDownEvent, IReturnOrderItemView returnOrderItemView)
    {
        if (dropDownEvent != null && dropDownEvent.SelectionItems != null && dropDownEvent.SelectionItems.Any())
        {
            var selectedReasonTypeOption = dropDownEvent.SelectionItems.First();
            returnOrderItemView.SKUType = selectedReasonTypeOption.Label;
            await OnSKUTypeChanged.InvokeAsync((returnOrderItemView, (selectedReasonTypeOption?.Label.ToString() == "NonSalable") ? Winit.Shared.Models.Enums.StockType.NonSalable : Winit.Shared.Models.Enums.StockType.Salable));
        }
        StateHasChanged();
    }
    private async Task OnReasonSelect(Winit.Shared.Models.Events.DropDownEvent dropDownEvent, IReturnOrderItemView returnOrderItemView)
    {
        if (dropDownEvent != null && dropDownEvent.SelectionItems != null && dropDownEvent.SelectionItems.Any())
        {
            var selectedReasonTypeOption = dropDownEvent.SelectionItems.First();
            returnOrderItemView.ReasonCode = selectedReasonTypeOption.Code;
            returnOrderItemView.ReasonText = selectedReasonTypeOption.Label;
        }
        StateHasChanged();
        await Task.CompletedTask;
    }
    public async Task OnUOMSelect(Winit.Shared.Models.Events.DropDownEvent dropDownEvent)
    {
        await OnSelectUOM.InvokeAsync(dropDownEvent);
        StateHasChanged();
    }
    public void OnCloneBtnClick()
    {
        OnGetCloneSelectionItem.InvokeAsync(selectedProduct);
        _isCloneItemOpen = true;
    }
    public void OnDeleteBtnClick()
    {
        OnClonedItemDelete.InvokeAsync(selectedProduct);
    }
    public void HandleRowBtnClick(Winit.Modules.ReturnOrder.Model.Interfaces.IReturnOrderItemView returnOrderItemView)
    {
        selectedProduct = returnOrderItemView;
        if (selectedProduct.ItemStatus == ItemState.Cloned)
        {
            OnDeleteBtnClick();
        }
        else
        {
            OnCloneBtnClick();
        }
    }
    public void OnQtyChange(string value)
    {
        if(value == null) return;
        decimal odQty = decimal.Parse(value);
        if (IsWithInvoice && odQty == selectedProduct.AvailableQty) return;
        selectedProduct.OrderQty = odQty;
        OnQtyChanged.InvokeAsync(selectedProduct);
    }
    private async Task OnReasonDDLClick(IReturnOrderItemView returnOrderItemView)
    {
        await _dropdownService.ShowDropDown(new Winit.UIComponents.Common.Services.DropDownOptions
        {
            Title = "Reason",
            DataSource = returnOrderItemView.ReasonsList,
            SelectionMode = Winit.Shared.Models.Enums.SelectionMode.Single,
            OnSelect = async (eventArgs) =>
            {
                // Your asynchronous handling logic goes here
                await OnReasonSelect(eventArgs, returnOrderItemView);
            },
        });
    }
    private async Task OnReasonTypeDDLClick(IReturnOrderItemView returnOrderItemView)
    {
        await _dropdownService.ShowDropDown(new Winit.UIComponents.Common.Services.DropDownOptions
        {
            Title = "SKU Type",
            DataSource = ReasonTypeSelectionItems,
            SelectionMode = Winit.Shared.Models.Enums.SelectionMode.Single,
            OnSelect = async (eventArgs) =>
            {
                // Your asynchronous handling logic goes here
                await OnReasonTypeSelect(eventArgs, returnOrderItemView);
            },
        });
    }
    private async Task OnUOMDDLClick(IReturnOrderItemView returnOrderItemView)
    {
        ;
        await _dropdownService.ShowDropDown(new Winit.UIComponents.Common.Services.DropDownOptions
        {
            Title = "UOM",
            UniqueUID = returnOrderItemView.UID,
            DataSource = OnGetDDLSelectionItem.Invoke(returnOrderItemView),
            SelectionMode = Winit.Shared.Models.Enums.SelectionMode.Single,
            OnSelect = async (eventArgs) =>
            {
                // Your asynchronous handling logic goes here
                await OnUOMSelect(eventArgs);
            },
        });
    }
}
