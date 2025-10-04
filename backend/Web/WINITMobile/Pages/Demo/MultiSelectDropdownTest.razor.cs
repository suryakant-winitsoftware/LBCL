using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using System;
using System.Collections.Generic;
using System.Linq;
using Winit.Modules.Common.BL;
using Winit.Shared.Models.Common;

namespace WINITMobile.Pages.Demo
{
    public partial class MultiSelectDropdownTest : ComponentBase
    {
        private Winit.Shared.Models.Enums.SelectionMode selectionMode; 
        private bool isMultiple;
        private string selectedItem;
        private List<ISelectionItem> data;
        private SelectionManager selectionManager;

        [Inject]
        private IJSRuntime JSRuntime { get; set; }

        protected override void OnInitialized()
        {
            selectionMode = Winit.Shared.Models.Enums.SelectionMode.Multiple;
            SetIsMultiple();
            // Initialize the data and selection manager
            data = new List<ISelectionItem>
        {
            new SelectionItem { Code = "001", Label = "Item 1" },
            new SelectionItem { Code = "002", Label = "Item 2" , IsSelected = true},
            new SelectionItem { Code = "003", Label = "Item 3" },
            new SelectionItem { Code = "004", Label = "Item 4" },
            new SelectionItem { Code = "005", Label = "Item 5" }
        };

            // Create a SelectionManager for single select
            selectionManager = new SelectionManager(data, selectionMode);
        }
        private void SetIsMultiple()
        {
            if (selectionMode == Winit.Shared.Models.Enums.SelectionMode.Multiple)
            {
                isMultiple = true;
            }
            else
            {
                isMultiple = false;
            }
        }
        private void OnSelectionChange(ChangeEventArgs e)
        {
            IEnumerable<string> selectedValue = GetSelectedValue(e.Value);
            if (selectedValue != null && selectedValue.Count() > 0)
            {
                foreach (string selectedCode in selectedValue)
                {
                    selectionManager.Select(data.FirstOrDefault(item => item.Code == selectedCode));
                }
            }
            GetSelectedItem();//Comment this later
        }
        private IEnumerable<string> GetSelectedValue(object? value)
        {
            if(isMultiple)
            {
                return value as IEnumerable<string>;
            }
            else
            {
                return  ((string)value).Split("|");
            }
        }
        private void GetSelectedItem()
        {
            List<ISelectionItem> selectedItems = selectionManager.GetSelectedSelectionItems();
            if(selectedItems != null && selectedItems.Count() > 0)
            {
                if(isMultiple)
                {
                    selectedItem = string.Join(",", selectedItems.Select(e => e.Label));
                }
                else
                {
                    selectedItem = selectedItems.FirstOrDefault().Label;
                }
            }
            else
            {
                selectedItem = "";
            }
            
        }
    }
}
