using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using Winit.Shared.Models.Events;
using Winit.UIComponents.Common.Services;

namespace Winit.UIComponents.Mobile.Common;
public partial class MobilePopUpDropDown
{
    public List<Winit.Shared.Models.Common.ISelectionItem> DataSource
    {
        get => dataSource;
        set => dataSource = value;
    }

    public Winit.Shared.Models.Enums.SelectionMode _SelectionMode { get; set; } = Winit.Shared.Models.Enums.SelectionMode.Single;

    public string Title { get; set; } = "";

    public required Func<DropDownEvent, Task> OnSelect { get; set; }

    public Func<DropDownEvent, Task> OnSelectAsync => async (eventArgs) =>
    {
        await OnSelect(eventArgs);
    };
    public required string UniqueUID { get; set; }
    public required List<Winit.Shared.Models.Common.ISelectionItem> dataSource { get; set; }
    public required Winit.Modules.Common.BL.SelectionManager selectionManager { get; set; }
    public required List<Winit.Shared.Models.Common.ISelectionItem> SelectedItems { get; set; }
    public bool ShowPopUp { get; set; } = false;
    public string OkBtnTxt { get; set; } = "Proceed";
    [Parameter] public bool ShowOtherOption { get; set; } = false;
    private bool IsOtherSelected => DataSource.Any(item => item.IsSelected && item.UID == "other");
    private string OtherReason { get; set; }
    public void GetLoad()
    {
        selectionManager = new Winit.Modules.Common.BL.SelectionManager(dataSource, _SelectionMode);
        if (_SelectionMode == Shared.Models.Enums.SelectionMode.Single)
        {
            SelectedItems = selectionManager.GetSelectedSelectionItems();
        }
    }

    protected override void OnInitialized()
    {
        // Subscribe to the service event to handle dropdown visibility
        _dropdownService.OnShowMobilePopUpDropDown += HandleShowDropDown;
        if (dataSource != null)
        {
            GetLoad();
        }
    }
    private async Task HandleShowDropDown(DropDownOptions options)
    {
        // Update component properties based on service event
        DataSource = options.DataSource;
        _SelectionMode = options.SelectionMode;
        Title = options.Title;
        OnSelect = options.OnSelect;
        UniqueUID = options.UniqueUID;
        ShowOtherOption = options.ShowOtherOption;
        selectionManager = new Winit.Modules.Common.BL.SelectionManager(options.DataSource, options.SelectionMode);
        SelectedItems = selectionManager.GetSelectedSelectionItems();
        GetLoad();
        // Show the popup
        ShowPopUp = true;
        OkBtnTxt = options.OkBtnTxt;
        StateHasChanged();
        await Task.CompletedTask;
    }

    private void OnSelectionChange(Winit.Shared.Models.Common.ISelectionItem item)
    {
        selectionManager.Select(item);
    }

    private void GetMultipleSelectedItems()
    {
        ShowPopUp = false;
        // SearchedItemsCopy = SelectedItems;
        SelectedItems = selectionManager.GetSelectedSelectionItems();
        DropDownEvent dropDownEvent = new()
        {
            UID = UniqueUID,
            SelectionMode = _SelectionMode,
            SelectionItems = SelectedItems
        };
        _ = OnSelect.Invoke(dropDownEvent);
        // this for disselect all
        selectionManager.DeselectAll();
    }

    private void CloseWithoutUpdating()
    {
        ShowPopUp = false;
        _ = OnSelect.Invoke(null);
    }
    private async Task Handle_ProceedClick()
    {
        ShowPopUp = false;
        if (IsOtherSelected && ShowOtherOption && !string.IsNullOrEmpty(OtherReason))
        {
            var otherItem = DataSource.FirstOrDefault(item => item.UID == "other");
            if (otherItem != null)
            {
                otherItem.Label = OtherReason;
            }
        }
        SelectedItems = selectionManager.GetSelectedSelectionItems();
        DropDownEvent dropDownEvent = new()
        {
            UID = UniqueUID,
            SelectionMode = _SelectionMode,
            SelectionItems = SelectedItems
        };
        await OnSelect.Invoke(dropDownEvent);
    }
}
