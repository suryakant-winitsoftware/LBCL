using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection.Metadata;
using System.Resources;
using System.Text.Json;
using Winit.Shared.Models.Common;
using Winit.UIComponents.Common.Language;
using Winit.UIComponents.Common.LanguageResources.Mobile;

namespace Winit.UIComponents.Common.CustomControles
{
    public partial class MultiSelect : ComponentBase
    {

        private static string buttonText { get; set; } = "Select PaymentMode   ◢";
        
        private static List<SelectionItem> listCopy = new List<SelectionItem>();
        private static List<SelectionItem> listFiltered = new List<SelectionItem>();
        private List<SelectionItem> listItem { get; set; } = new List<SelectionItem>();
        private List<SelectionItem> listItem1 { get; set; } = new List<SelectionItem>();
        [Parameter] public List<SelectionItem> data { get; set; } = new List<SelectionItem>();

        [Parameter] public EventCallback<List<SelectionItem>> OnSelect { get; set; }

        
        protected override async Task OnInitializedAsync()
        {
            listCopy = data;
            shn.showing = false;
            LoadResources(null, _languageService.SelectedCulture);

        }

       

        protected void LoadResources(object sender, string culture)
        {
            CultureInfo cultureInfo = new CultureInfo(culture);
            ResourceManager resourceManager = new ResourceManager("Winit.UIComponents.Common.LanguageResources.Mobile.LanguageKeys", typeof(Winit.UIComponents.Common.LanguageResources.Mobile.LanguageKeys).Assembly);
            Localizer = new CustomStringLocalizer<LanguageKeys>(resourceManager, cultureInfo);
        }

        private void HandleCheckboxChange(SelectionItem item)
        {
            item.IsSelected = !item.IsSelected;
            HandleSelection(item);
        }

        private void HandleSelection(SelectionItem selectedItem)
        {
            listItem1 = data;
            foreach (var li in data)
            {
                if (li.IsSelected && li.Label == selectedItem.Label)
                {
                    // Item is not already in the list, add it
                    listItem.Add(selectedItem);
                }
                else if (li.Label == selectedItem.Label)
                {
                    listItem.Remove(selectedItem);
                }
            }
            buttonText = listItem.Count == 0 ? @Localizer["select_paymentmode"] : listItem.Count == 1 ? listItem.Count + @Localizer["items_selected"] : listItem.Count + @Localizer["items_selected"];
            OnSelect.InvokeAsync(listItem1);
        }

        public void Reset()
        {
            buttonText = @Localizer["select_paymentmode"];
        }
        private static void ShowTable()
        {
            shn.showing = !shn.showing;

        }
        private void ToggleSelectAll(ChangeEventArgs e)
        {
            foreach (var item in data)
            {
                item.IsSelected = Convert.ToBoolean(e.Value);
            }
            listItem = new();
            UpdateSelectedItems();
            OnSelect.InvokeAsync(data);
        }

        private void UpdateSelectedItems()
        {
            try
            {
                var selectedItems = data.Where(item => item.IsSelected).Select(item => item.Label).ToList();

                buttonText = selectedItems.Count == 0 ? @Localizer["select_paymentmode"] : $"{selectedItems.Count} {@Localizer["items_selected"]}";
            }
            catch (Exception ex)
            {

            }
        }
        private void searchClicked(ChangeEventArgs e)
        {
            if (e.Value.ToString() != null)
            {
                listFiltered = listCopy.Where(item =>
            item.Label.Contains(e.Value.ToString(), StringComparison.OrdinalIgnoreCase)).ToList();
                data = listFiltered;

            }
            else
            {
                data = listCopy;
            }
        }
        public class shn
        {
            public static bool showing { get; set; } = false;

            public static void CloseDropDown()
            {
                showing = false;
            }
        }
        
    }
}
