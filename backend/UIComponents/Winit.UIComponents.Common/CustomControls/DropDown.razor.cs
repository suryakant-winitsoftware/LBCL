using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Localization;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Resources;
using System.Text;
using System.Threading.Tasks;
using Winit.Modules.Common.BL;
using Winit.Shared.Models.Common;
using Winit.Shared.Models.Enums;
using Winit.Shared.Models.Events;
using Winit.UIComponents.Common.Language;
using Winit.UIComponents.Common.LanguageResources.Mobile;

namespace Winit.UIComponents.Common.CustomControls
{
    public partial class DropDown
    {
        [Parameter]
        public List<Winit.Shared.Models.Common.ISelectionItem> DataSource
        {
            get
            {
                return dataSource;
            }
            set
            {
                dataSource = value;
                if (IsButtonVisible)
                {
                    GetLoad();
                }
            }

        }
        [Parameter]
        public Winit.Shared.Models.Enums.SelectionMode _SelectionMode { get; set; } = Winit.Shared.Models.Enums.SelectionMode.Single;
        [Parameter]
        public string Title { get; set; } = "";
        [Parameter]
        public bool Disabled { get; set; }
        [Parameter]
        public bool IsSearchable { get; set; } = true;
        [Parameter]
        public EventCallback<DropDownEvent> OnSelect { get; set; }
        [Parameter]
        public bool IsButtonVisible { get; set; } = false;
        [Parameter]
        public string? UniqueUID { get; set; }
        public List<Winit.Shared.Models.Common.ISelectionItem> dataSource { get; set; }
        public Winit.Modules.Common.BL.SelectionManager? selectionManager { get; set; }
        private List<Winit.Shared.Models.Common.ISelectionItem> SelectedItems { get; set; }
        private string? Label { get; set; } = "Select item";
        private List<Winit.Shared.Models.Common.ISelectionItem> SearchedItems { get; set; }
        [Parameter]
        public bool ShowPopUp { get; set; } = false;
        [Parameter]
        public bool IsVewOptionVisible { get; set; }
        bool IsView { get; set; }
        protected override void OnInitialized()
        {
            ShowPopUp = IsButtonVisible ? false : true;
            GetLoad();
        }
        protected override async Task OnInitializedAsync()
        {
            LoadResources(null, _languageService.SelectedCulture);
        }
        protected void LoadResources(object sender, string culture)
        {
            CultureInfo cultureInfo = new CultureInfo(culture);
            ResourceManager resourceManager = new ResourceManager("Winit.UIComponents.Common.LanguageResources.Mobile.LanguageKeys", typeof(Winit.UIComponents.Common.LanguageResources.Mobile.LanguageKeys).Assembly);
            Localizer = new CustomStringLocalizer<LanguageKeys>(resourceManager, cultureInfo);
        }
        private void UpdateSelectionManager(List<Winit.Shared.Models.Common.ISelectionItem> newDataSource)
        {
            selectionManager = new Winit.Modules.Common.BL.SelectionManager(newDataSource, _SelectionMode);
            Label = "Select" + " " + Title;

            SearchedItems = DataSource;
            SelectedItems = selectionManager!.GetSelectedSelectionItems();
            if (SelectedItems == null || !SelectedItems.Any()) return;
            if (_SelectionMode == Shared.Models.Enums.SelectionMode.Single)
            {
                Label = SelectedItems.First()?.Label;
            }
            else
            {
                Label = SelectedItems.Count().ToString() + "records selected";
            }

        }
        public async Task UpdateDataSource(List<ISelectionItem> newDataSource, SelectionMode selectionMode)
        {
            DataSource = newDataSource;
            UpdateSelectionManager(newDataSource);
        }
        public void GetLoad()
        {
            selectionManager = new Winit.Modules.Common.BL.SelectionManager(dataSource, _SelectionMode);
            Label = $"Select {Title}";

            SearchedItems = DataSource;
            SelectedItems = selectionManager!.GetSelectedSelectionItems();
            if (SelectedItems == null || !SelectedItems.Any()) return;
            if (_SelectionMode == Shared.Models.Enums.SelectionMode.Single)
            {
                Label = SelectedItems.First()?.Label;
            }
            else
            {
                Label = SelectedItems.Count().ToString() + "records selected";
            }
            StateHasChanged();
        }
        protected void SetLabelOnSingleSelection()
        {

        }
        protected void ShowPopup_ButtonClick()
        {
            SelectedItems = selectionManager!.GetSelectedSelectionItems();
            SearchedItems = DataSource;
            foreach (ISelectionItem item in SearchedItems)
            {
                item.IsSelected_InDropDownLevel = item.IsSelected;
            }

            if (Disabled)
            {
                SearchedItems = SearchedItems.OrderByDescending(p => p.IsSelected).ToList();
            }

            ShowPopUp = true;
        }

        private async Task OnSelectionChange(Winit.Shared.Models.Common.ISelectionItem item)
        {

            selectionManager?.Select(item);

            if (_SelectionMode == Winit.Shared.Models.Enums.SelectionMode.Single)
            {
                ShowPopUp = false;
                SelectedItems = selectionManager!.GetSelectedSelectionItems();
                Label = SelectedItems?.Count > 0 ? item.Label : $"Select {Title}";
                DropDownEvent dropDownEvent = new DropDownEvent
                {
                    UID = UniqueUID,
                    SelectionMode = _SelectionMode,
                    SelectionItems = SelectedItems
                };
                await OnSelect.InvokeAsync(dropDownEvent);
            }
            else
            {
                Label = $"{selectionManager?.GetSelectedSelectionItems().Count} record(s) selected";
            }
        }

        private void Search(string searchTerm)
        {

            if (!string.IsNullOrEmpty(searchTerm))
            {

                SearchedItems = DataSource.Where(item => item.Label.Contains(searchTerm, StringComparison.OrdinalIgnoreCase)).ToList();
            }
            else
            {
                SearchedItems = DataSource;
            }
        }

        private async Task GetMultipleSelectedItems()
        {
            ShowPopUp = false;
            if (!Disabled)
            {
                SelectedItems = selectionManager!.GetSelectedSelectionItems();
                if (SelectedItems != null && SelectedItems.Count > 0)
                {
                    foreach (ISelectionItem item in SelectedItems)
                    {
                        item.IsSelected_InDropDownLevel = item.IsSelected;
                    }
                }
                DropDownEvent dropDownEvent = new DropDownEvent
                {
                    UID = UniqueUID,
                    SelectionMode = _SelectionMode,
                    SelectionItems = SelectedItems
                };
                Label = SelectedItems?.Count + " " + "records selected";
                await OnSelect.InvokeAsync(dropDownEvent);
            }
        }

        private void CheckORUncheckAll(bool Check_Uncheck)
        {
            foreach (var item in SearchedItems)
            {
                item.IsSelected = !Check_Uncheck;
                selectionManager?.Select(item);
            }
            int selectedCount = selectionManager!.GetSelectedSelectionItems().Count;
            Label = selectedCount > 0 ? selectedCount + "records selected" : "Select" + " " + Title;
        }

        private void CloseWithoutUpdating()
        {
            ShowPopUp = false;
            if (IsView)
            {
                IsView = false;
                return;
            }
            var data = selectionManager?.GetAllData();
            foreach (ISelectionItem selectionItem in data)
            {
                selectionItem.IsSelected = selectionItem.IsSelected_InDropDownLevel;
            }
            //Label = (selectionManager?.GetAllData())?.Count(p=>p.IsSelected_InDropDownLevel==true) + " " + @Localizer["records_selected"];
            DataSource = data;
            OnSelect.InvokeAsync(null);
        }
        public void ShowPopup(bool isVisible)
        {
            ShowPopUp = isVisible;
        }

        public void DeSelectAll()
        {
            selectionManager?.DeselectAll();
            Label = "Select" + " " + Title;
            StateHasChanged();
        }
    }
}
