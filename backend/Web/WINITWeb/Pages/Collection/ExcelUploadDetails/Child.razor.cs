using Microsoft.AspNetCore.Components;
using static Winit.Modules.CollectionModule.Model.Interfaces.IAccCollection;
using Winit.Modules.CollectionModule.Model.Classes;
using Winit.Modules.CollectionModule.Model.Interfaces;
using Winit.Shared.Models.Common;
using Winit.Shared.Models.Enums;
using Winit.Shared.Models.Events;

namespace WinIt.Pages.Collection.ExcelUploadDetails
{
    public partial class Child
    {
        [Parameter]
        public List<Collections> People { get; set; } = new List<Collections>();

        [Parameter]
        public List<string> Recipt { get; set; } = new List<string>();
        [Parameter]
        public AccCollectionAllotment Items { get; set; } = new AccCollectionAllotment();
        [Parameter]
        public bool ShowList { get; set; } = true;
        [Parameter]
        public EventCallback<string> OnItemSelected { get; set; }
        private static AccCollectionAllotment[] Array { get; set; }
        private AccCollectionAllotment[] Array1 { get; set; }
        private IAccCollectionAllotment list { get; set; }
        private static IAccCollectionAllotment list1 { get; set; }
        private string Selected { get; set; } = "--Select Customer--";
        private static string Selecteditem { get; set; } = "--Select--";
        private static AccCollectionAllotment stst1 { get; set; }
        [Parameter]
        public AccCollectionAllotment[] obj
        {
            get
            {
                return Array1;
            }
            set
            {
                if (value != null)
                {
                    Array1 = value;
                }
            }
        }
        [Parameter]
        public IAccCollectionAllotment stst
        {
            get
            {
                return list;
            }
            set
            {
                if (value != null)
                {
                    list = value;
                }
            }
        }
        private static AccCollectionAllotment Paren { get; set; } = new AccCollectionAllotment();
        [Parameter]
        public EventCallback<AccCollectionAllotment[]> OnDataSent { get; set; }
        private bool show { get; set; } = false;
        private bool ShowCustomers { get; set; } = false;
        private string selectedValueText { get; set; } = "";
        List<ISelectionItem> customerData { get; set; } = new List<ISelectionItem>();
        protected override void OnInitialized()
        {
            foreach (var data in Recipt)
            {
                SelectionItem type = new SelectionItem
                {
                    Code = data,
                    Label = data,
                };
                customerData.Add(type);
            }
        }
        //dropdown 
        private void DropNow()
        {
            try
            {
                ShowCustomers = !ShowCustomers;
            }
            catch (Exception ex)
            {

            }
        }
        //when selects a item in dropdown gets hit
        private async Task selectedItem(string item)
        {
            try
            {
                Selected = item;
                Selecteditem = Selected;
                await OnItemSelected.InvokeAsync(item); // Send the selected item to the parent
            }
            catch (Exception ex)
            {

            }
        }

        private async Task OnSelected(DropDownEvent dropDownEvent, string type)
        {

            if (dropDownEvent != null)
            {
                if (dropDownEvent.SelectionMode == SelectionMode.Single && dropDownEvent.SelectionItems != null && dropDownEvent.SelectionItems.Count > 0)
                {
                    await selectedItem(dropDownEvent.SelectionItems.First().Code);
                     ShowCustomers = false;
                    StateHasChanged();
                }
            }
            else
            {
                ShowCustomers = false;
            }
        }
    }
}
