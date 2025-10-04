using Microsoft.AspNetCore.Components;
using Microsoft.Identity.Client.Extensibility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using Winit.Modules.FileSys.Model.Interfaces;
using Winit.Modules.ReturnOrder.Model.Interfaces;
using WINITMobile.Data;
using WINITMobile.Pages.Base;
using System.Globalization;
using System.Resources;
using Winit.UIComponents.Common.Language;
using Winit.UIComponents.Common.LanguageResources.Mobile;
namespace WINITMobile.Pages.Return;

partial class ReturnOrderListView : BaseComponentBase
{
    [Parameter]
    public List<Winit.Modules.ReturnOrder.Model.Interfaces.IReturnOrderItemView> DisplayReturnOrderItemViews { get; set; }
    [Parameter]
    public EventCallback<(Winit.Modules.ReturnOrder.Model.Interfaces.IReturnOrderItemView, Winit.Shared.Models.Enums.StockType)> OnResonTypeChange { get; set; }
    [Parameter]
    public EventCallback<Winit.Modules.ReturnOrder.Model.Interfaces.IReturnOrderItemView> OnQtyChanged { get; set; }
    [Parameter]
    public EventCallback<Winit.Modules.ReturnOrder.Model.Interfaces.IReturnOrderItemView> OnDelete { get; set; }
    [Parameter]
    public List<Winit.Shared.Models.Common.ISelectionItem> UOMSelectionItems { get; set; }
    [Parameter]
    public EventCallback<Winit.Shared.Models.Events.DropDownEvent> DropDownSelection_OnSingleSelect { get; set; }
    [Parameter]
    public EventCallback<Winit.Modules.ReturnOrder.Model.Interfaces.IReturnOrderItemView> OnUOMDDClick { get; set; }
    [Parameter]
    public string ImgFolderPath { get; set; }
    [Parameter]
    public EventCallback<(string fileName, string folderPath)> OnImageCapture { get; set; }
    [Parameter]
    public EventCallback<string> OnImageDeleteClick { get; set; }
    [Parameter]
    public bool IsWithInvoice { get; set; }
    private readonly TimeSpan debounceDelay = TimeSpan.FromMilliseconds(500);
    private Timer debounceTimer;
    public Winit.Modules.ReturnOrder.Model.Interfaces.IReturnOrderItemView SelectedProduct { get; set; }
    private bool IsDeletePopUpOpen { get; set; }
    private bool IsOpenUOM { get; set; }
    private FileCaptureData fileCaptureData = new FileCaptureData
    {
        AllowedExtensions = new List<string> { ".jpg", ".png" }, // Add allowed extensions
        IsCameraAllowed = true,
        IsGalleryAllowed = true,
        MaxNumberOfItems = 5,
        MaxFileSize = 10 * 1024 * 1024, // 10 MB
        EmbedLatLong = true,
        EmbedDateTime = true,
        LinkedItemType = "ItemType",
        LinkedItemUID = "ItemUID",
        EmpUID = "EmployeeUID",
        JobPositionUID = "JobPositionUID",
        IsEditable = true,
        Files = new List<FileSys>()
    };
    protected override void OnInitialized()
    {
        LoadResources(null, _languageService.SelectedCulture);
    }

    private void ReasonTypeChanged(Winit.Modules.ReturnOrder.Model.Interfaces.IReturnOrderItemView selectionItem, ChangeEventArgs e)
    {
        OnResonTypeChange.InvokeAsync((selectionItem, (e?.Value.ToString() == "NonSalable") ? Winit.Shared.Models.Enums.StockType.NonSalable : Winit.Shared.Models.Enums.StockType.Salable));
        selectionItem.ReasonCode = null;
        selectionItem.ReasonText = null;
    }
    public void OnUOMChange(Winit.Shared.Models.Events.DropDownEvent dropDownEvent)
    {
        DropDownSelection_OnSingleSelect.InvokeAsync(dropDownEvent);
        IsOpenUOM = false;
    }
    private void OnUOMButtonClick(Winit.Modules.ReturnOrder.Model.Interfaces.IReturnOrderItemView returnOrderItemView)
    {
        SelectedProduct = returnOrderItemView;
        OnUOMDDClick.InvokeAsync(returnOrderItemView);
        IsOpenUOM = true;
    }

    private void OnSearchInput(Winit.Modules.ReturnOrder.Model.Interfaces.IReturnOrderItemView returnOrderItemView, ChangeEventArgs args)
    {
        string inputValue = args.Value.ToString();
        if (decimal.TryParse(inputValue, out decimal parsedValue))
        {
            if (IsWithInvoice && parsedValue > returnOrderItemView.AvailableQty)
            {

                return;
            }
            returnOrderItemView.OrderQty = parsedValue;
            // Use the Debouncer to debounce the PerformSearch method with the parsedValue parameter
            Winit.Shared.CommonUtilities.Common.CommonFunctions
                .Debounce<Winit.Modules.ReturnOrder.Model.Interfaces.IReturnOrderItemView>(PerformSearch,
                returnOrderItemView, TimeSpan.FromMilliseconds(300));
        }
    }

    private void PerformSearch(Winit.Modules.ReturnOrder.Model.Interfaces.IReturnOrderItemView returnOrderItemView)
    {
        InvokeAsync(() =>
        {
            OnQtyChanged.InvokeAsync(returnOrderItemView);
            StateHasChanged();
        });
    }

    private async Task OnReasonDDLClick(IReturnOrderItemView returnOrderItemView)
    {
        await _dropdownService.ShowDropDown(new Winit.UIComponents.Common.Services.DropDownOptions
        {
            DataSource = returnOrderItemView.ReasonsList,
            SelectionMode = Winit.Shared.Models.Enums.SelectionMode.Single,
            OnSelect = async (eventArgs) =>
            {
                // Your asynchronous handling logic goes here
                await OnReasonSelectedChanged(eventArgs, returnOrderItemView);
            },
        });
    }
    private async Task OnReasonSelectedChanged(Winit.Shared.Models.Events.DropDownEvent dropDownEvent, Winit.Modules.ReturnOrder.Model.Interfaces.IReturnOrderItemView selectionItem)
    {
        if (dropDownEvent != null && dropDownEvent.SelectionItems != null && dropDownEvent.SelectionItems.Any())
        {
            string reasonCode = dropDownEvent.SelectionItems.FirstOrDefault().Code;
            string reasonText = dropDownEvent.SelectionItems.FirstOrDefault().Label;
            selectionItem.ReasonCode = reasonCode;
            selectionItem.ReasonText = reasonText;
            DisplayReturnOrderItemViews.Where(i => i.ReasonCode == null || i.ReasonText == null)
                                       .ToList()
                                       .ForEach(j => { j.ReasonText = reasonText; j.ReasonCode = reasonCode; j.ReasonsList.Find(k => k.Code == reasonCode).IsSelected = true; });
        }
        StateHasChanged();
        //selectionItem.IsReasonDDOpen = false;
        await Task.CompletedTask;
    }

    private async Task OnProductDelete(IReturnOrderItemView returnOrderItemView)
    {
        if (await _alertService.ShowConfirmationReturnType(@Localizer["delete"], @Localizer["are_you_sure._you_want_to_delete_this_item"], @Localizer["yes"], @Localizer["no"]))
        {
            await OnDelete.InvokeAsync(returnOrderItemView);
        }
    }

}

