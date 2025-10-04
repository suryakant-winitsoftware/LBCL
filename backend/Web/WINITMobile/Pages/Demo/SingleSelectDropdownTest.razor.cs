using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using System;
using System.Collections.Generic;
using System.Linq;
using Winit.Modules.Common.BL;
using Winit.Shared.Models.Common;

namespace WINITMobile.Pages.Demo
{
    public partial class SingleSelectDropdownTest : ComponentBase
    {
        private string selectedItem;
        private List<ISelectionItem> data;
        private SelectionManager selectionManager;
        protected override void OnInitialized()
        {
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
            selectionManager = new SelectionManager(data, Winit.Shared.Models.Enums.SelectionMode.Single);
        }
        private void OnSelectionChange(ChangeEventArgs e)
        {
            selectionManager.Select(data.FirstOrDefault(item => item.Code == e.Value.ToString()));
            selectedItem = e.Value.ToString();
        }
    }
}
