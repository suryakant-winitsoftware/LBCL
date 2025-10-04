using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using DistributionManagement.Models;
using Microsoft.AspNetCore.Components;
using MudBlazor;



namespace DistributionManagement
{
    public partial class BulkSalesOrder
    {
        private string customerStoredName = "";
        private List<SelectCategoryType> displayedCategoryType;
        private bool SelectCategoryTypeddl = false;
        private bool SelectCategoryddl = false;

        private int CategoryTypecount = 0;
        private int Categorycount = 0;
        private List<SelectCategory> selectedCategoryList = new List<SelectCategory>(); // Initialize the list
        private List<SelectCategoryType> listselectCategoryTypes = new List<SelectCategoryType>(); // Initialize the list
        private List<SKUCategory> DisplayedSKUList = new List<SKUCategory>();
        private List<CustomerProduct> selectedCustomerProducts { get; set; } = new List<CustomerProduct>();
        private List<CustomerProduct> selectedCustomerProductsSearch { get; set; } = new List<CustomerProduct>();
        private List<DraftRecordClass> draftRecords = new List<DraftRecordClass>();

        private string searchTerm = "";

        private string customerStoredId = "";
        private string alertMessage = "";
        private bool showAlert = false;
        private bool isSearchActive = false;
        private bool CheckAllRows1 { get; set; } = false;
        private bool CheckAllRows { get; set; } = false;



        private List<RouteItem> displayedRoutes = new List<RouteItem>();
        private bool routeisDropdownOpen = false;

        private string routeDropdownStyle => routeisDropdownOpen ? "display: block;" : "display: none;";
        private Dictionary<SKUCategory, bool> rowCheckStatus = new Dictionary<SKUCategory, bool>();
        private string DropdownStyleSCT => SelectCategoryTypeddl ? "display: block;" : "display: none;";
        private string DropdownStyleSC => SelectCategoryddl ? "display: block;" : "display: none;";

        private bool isDialogOpen = false;
        private bool selectAllRows = false;
        private List<SKUCategory> OriginalDisplayedSKUList;

        private FormData formData = new FormData();
        bool showPromoRecords = false;

        private bool showCheckedProductInTable = false;
        private List<SKUCategory> checkedlist { get; set; } = new List<SKUCategory>();



        private MudTable<SKUCategory> table;
        private MudTable<CustomerProduct> table1;

        void OnCheckboxChange(ChangeEventArgs e)
        {

            showPromoRecords = (bool)e.Value;
            if (showPromoRecords)
            {
                OriginalDisplayedSKUList = DisplayedSKUList;
                DisplayedSKUList = DisplayedSKUList.Where(dItem => dItem.IsPromo).ToList();
            }
            else
            {
                DisplayedSKUList = OriginalDisplayedSKUList;
            }
        }



        private void SelectCategoryTypeDropDown()
        {

            SelectCategoryTypeddl = !SelectCategoryTypeddl;

        }
        private void SelectCategoryDropDown()
        {

            SelectCategoryddl = !SelectCategoryddl;

        }

        private void CloseAlert(object state = null)
        {
            showAlert = false;
            StateHasChanged(); // Update the UI to reflect the change
        }

        private async Task ToggleSelectAllRows()
        {
            foreach (var item in DisplayedSKUList)
            {
                rowCheckStatus[item] = selectAllRows;
            }
        }


        private void CancelSelectedProduct()
        {
            selectedCustomerProducts.Clear();
            selectedCustomerProductsSearch.Clear();
        }



        private void DeleteSelectedProduct()
        {
            var options = new DialogOptions { CloseOnEscapeKey = true };
            var parameters = new DialogParameters();
            parameters.Add("Callback", new Action<bool>(HandleDeleteConfirmed));
            DialogService.Show<DialogUsageExample_Dialog>("Simple Dialog", parameters, options);
        }



        private void AddToDisplayedSKUList(SKUCategory item)
        {
            // Create a copy of the item and add it to the DisplayedSKUList
            DisplayedSKUList.Add(new SKUCategory
            {
                Id = item.Id,
                Name = item.Name,
                // Copy other properties as needed
            });
        }
        private void AddToCheckedList(SKUCategory item)
        {
            // Create a copy of the item and add it to the checkedlist
            checkedlist.Add(new SKUCategory
            {
                Id = item.Id,
                Name = item.Name,
                // Copy other properties as needed
            });
        }


        private void AddCheckedProduct()
        {
            showCheckedProductInTable = true;
            isDialogOpen = false;
            foreach (var data in checkedlist)
            {
                var customerProduct = new CustomerProduct
                {
                    Id = customerStoredId,
                    Name = customerStoredName,
                    SKUCode = data.Id.ToString(),
                    SKUDescription = data.Name,
                    SKUqty = 0
                };

                selectedCustomerProducts.Add(customerProduct);
            }
            selectedCustomerProductsSearch = new List<CustomerProduct>(selectedCustomerProducts);

            DisplayedSKUList = SKUCategoryList;
        }

        private void HandleRowSelectionChange(SKUCategory context, bool newValue)
        {
            if (newValue)
            {
                context.IsChecked = newValue;
                checkedlist.Add(context);
            }
            else
            {
                context.IsChecked = newValue;
                checkedlist.Remove(context);
            }
        }

        private void HandleRowSelectionChangePopulate(CustomerProduct context, bool newValue)
        {
            if (newValue)
            {
                context.IsActive = newValue;
                //checkedlist.Add(context);
            }
            else
            {
                context.IsActive = newValue;
                // checkedlist.Remove(context);
            }
        }


        private void HandleDeleteConfirmed(bool isConfirmed)
        {
            if (isConfirmed)
            {
                if (selectedCustomerProducts.Count > 0)
                {
                    selectedCustomerProducts.RemoveAll(product => product.IsActive);
                }

            }
            else
            {
                selectedCustomerProducts.ForEach(product => product.IsActive = false);
            }
            StateHasChanged();
        }




        private void PerformSearchSCT(ChangeEventArgs e)
        {
            searchTerm = e.Value.ToString();
            displayedCategoryType = selectCategoryTypes
                .Where(route => string.IsNullOrWhiteSpace(searchTerm) || route.Name.Contains(searchTerm, StringComparison.OrdinalIgnoreCase))
                .ToList();
        }
        private void PerformSearchSC(ChangeEventArgs e)
        {
            searchTerm = e.Value.ToString();
            selectedCategoryList = selectedCategoryList
                .Where(route => string.IsNullOrWhiteSpace(searchTerm) || route.Name.Contains(searchTerm, StringComparison.OrdinalIgnoreCase))
                .ToList();
        }

        private void routePerformSearch(ChangeEventArgs e)
        {
            searchTerm = e.Value.ToString();
            displayedRoutes = RoutesData
                .Where(route => string.IsNullOrWhiteSpace(searchTerm) || route.Name.Contains(searchTerm, StringComparison.OrdinalIgnoreCase))
                .ToList();
        }

        private void CustomerPerformSearch(ChangeEventArgs e)
        {
            searchTerm = e.Value.ToString();
            FilteredCustomer = CustomerData
                .Where(route => string.IsNullOrWhiteSpace(searchTerm) || route.Name.Contains(searchTerm, StringComparison.OrdinalIgnoreCase))
                .ToList();
        }



        private void SearchAmongSelectedProduct(ChangeEventArgs e)
        {
            searchTerm = e.Value.ToString();

            if (string.IsNullOrWhiteSpace(searchTerm))
            {
                // If the search term is empty, reset the list to the original data
                selectedCustomerProducts = new List<CustomerProduct>(selectedCustomerProductsSearch);
            }
            else
            {
                // Filter a copy of the original data based on the search term
                selectedCustomerProducts = selectedCustomerProductsSearch
                    .Where(product =>
                        product.Id.Contains(searchTerm, StringComparison.OrdinalIgnoreCase) ||
                        product.Name.Contains(searchTerm, StringComparison.OrdinalIgnoreCase) ||
                        product.SKUCode.Contains(searchTerm, StringComparison.OrdinalIgnoreCase) ||
                        product.SKUDescription.Contains(searchTerm, StringComparison.OrdinalIgnoreCase))
                    .ToList();
            }
        }





        private void HandleAllSelectionChange(bool newValue)
        {
            CheckAllRows = newValue;

            foreach (var item in DisplayedSKUList)
            {
                item.IsChecked = newValue;
                if (newValue)
                {
                    // Add items to checkedlist if they are checked
                    if (!checkedlist.Contains(item))
                    {
                        checkedlist.Add(item);
                    }
                }
                else
                {
                    // Remove items from checkedlist if they are unchecked
                    if (checkedlist.Contains(item))
                    {
                        checkedlist.Remove(item);
                    }
                }
            }
        }

        private void HandleAllSelectionChangePopulate(bool newValue)
        {
            CheckAllRows1 = newValue;


            foreach (var item in selectedCustomerProducts)
            {
                item.IsActive = newValue;
                if (newValue)
                {
                    // Remove items from checkedlist if they are unchecked
                    if (selectedCustomerProducts.Contains(item))
                    {
                        selectedCustomerProducts.Remove(item);
                    }

                    // Add items to checkedlist if they are checked
                    //if (!checkedlist.Contains(item))
                    //{
                    //    checkedlist.Add(item);
                    //}
                }
                else
                {
                    // Remove items from checkedlist if they are unchecked
                    //if (checkedlist.Contains(item))
                    //{
                    //    checkedlist.Remove(item);
                    //}
                }
            }

        }

        private void ToggleCheckboxCategoryType(string itemId)
        {
            DisplayedSKUList = SKUCategoryList;
            foreach (var obj in selectedCategoryList)
            {
                obj.IsSelected = false;
            }
            Categorycount = 0;
            var selectedType = selectCategoryTypes.FirstOrDefault(item => item.Id == itemId);

            if (selectedType != null)
            {
                if (selectedType.IsSelected)
                {
                    // Checkbox is being unchecked, so decrement count and remove related categories
                    CategoryTypecount--;
                    var relatedCategoryIds = selectCategories
                        .Where(category => category.ParentId == itemId)
                        .Select(category => category.Id)
                        .ToList();

                    selectedCategoryList.RemoveAll(category => relatedCategoryIds.Contains(category.Id));
                }
                else
                {
                    // Checkbox is being checked, so increment count and add related categories
                    CategoryTypecount++;
                    var relatedCategories = selectCategories.Where(category => category.ParentId == itemId);
                    selectedCategoryList.AddRange(relatedCategories);
                }

                // Toggle the selection status
                selectedType.IsSelected = !selectedType.IsSelected;
            }
        }
        private void ToggleCheckboxCategory(string itemId)
        {
            var selectedType = selectCategories.FirstOrDefault(item => item.Id == itemId);


            if (selectedType != null)
            {
                if (selectedType.IsSelected)
                {
                    // Checkbox is being unchecked, so decrement count and remove related categories
                    Categorycount--;
                    var relatedCategoryIds = SKUCategoryList
                         .Where(category => category.ParentId == itemId)
                         .Select(category => category.Id)
                         .ToList();

                    DisplayedSKUList.RemoveAll(category => relatedCategoryIds.Contains(category.Id));
                }
                else
                {
                    // Checkbox is being checked, so increment count and add related categories
                    Categorycount++;
                    var relatedCategories = SKUCategoryList.Where(category => string.Equals(category.ParentId, itemId));
                    if (Categorycount == 1)
                    {
                        DisplayedSKUList = new List<SKUCategory>();
                    }
                    DisplayedSKUList.AddRange(relatedCategories);
                }

                //Toggle the selection status
                selectedType.IsSelected = !selectedType.IsSelected;
            }
        }
        private void OpenDialog()
        {
            if (string.IsNullOrEmpty(customerStoredId))
            {
                showAlert = true;
                alertMessage = "Select At least one customer";
                // Start a timer to close the alert after 2 seconds (2000 milliseconds)
                TimerCallback timerCallback = _ => CloseAlert();
                var timer = new Timer(timerCallback, null, 2000, Timeout.Infinite);
            }
            else
            {
                isDialogOpen = true;
                displayedCategoryType = selectCategoryTypes;
                DisplayedSKUList = SKUCategoryList;
            }
        }

        private void CloseDialog()
        {
            isDialogOpen = false;
        }

        private void routeToggleDropdown()
        {
            routeisDropdownOpen = !routeisDropdownOpen;
            displayedRoutes = RoutesData;
        }





        private async Task DrafRecordsMethods()
        {
            foreach (var data in selectedCustomerProducts)
            {
                var draftRecord = new DraftRecordClass
                {
                    Id = data.Id,
                    Name = data.Name,
                    SKUCode = data.SKUCode,
                    SKUDescription = data.SKUDescription,
                    SKUQTY = data.SKUqty.ToString(),
                    SKuUOM = data.SKUuom ?? "EU",
                    OrderDate  = formData.selectedOrderDate,
                    DeliveryDate = formData.selectedDeliveryDate,
                    OrderType = formData.selectedOrderType,
                    Route = SelectedRoute?.Name

                };
                draftRecords.Add(draftRecord);
                var serializedDrafts = JsonSerializer.Serialize(draftRecords);
                await SessionStorage.SetItemAsync("drafts", serializedDrafts);
            }
            
        }

        private async Task LoadDrafts()
        {
            // Retrieve and deserialize drafts from sessionStorage
            var serializedDrafts = await SessionStorage.GetItemAsync<string>("drafts");
            if (!string.IsNullOrWhiteSpace(serializedDrafts))
            {
                draftRecords = JsonSerializer.Deserialize<List<DraftRecordClass>>(serializedDrafts);
            }
        }

       


        protected override void OnInitialized()
        {
            
            formData.selectedOrderType = "forwardDate";
            formData.selectedOrderDate = DateTime.Today;
            if(formData.selectedOrderType != "backDate")
            {
                formData.selectedDeliveryDate = formData.selectedOrderDate?.AddDays(1);
            }
        else
            {
                formData.selectedDeliveryDate = DateTime.Today;
            }
           

            base.OnInitialized();
        }

        private async Task Check()
        {
            LoadDrafts();
        }


      

        private void ToggleSearch()
        {
            isSearchActive = true;
        }

       


       
        


        //private string selectedCode;



        

        private bool isDropdownOpen = false;

        //private int selectedItemId = -1;

        private List<Item> CustomerData = new List<Item>
    {
        new Item { RouteId="r1",Id = "1", Name = "First Customerid1", IsSelected = false },
       new Item { RouteId="r1", Id = "2", Name = "First Customerid2", IsSelected = false },
        new Item { RouteId="r2", Id = "3", Name = "Second Customerid1", IsSelected = false },
        new Item { RouteId="r2", Id = "4", Name = "Second Customerid2", IsSelected = false },
        new Item { RouteId="r3", Id = "5", Name = "Third Customerid1", IsSelected = false },

        // Add more items as needed
    };

        private List<Item> _filteredItems = new List<Item>(); 
        private List<Item> FilteredCustomer
        {
            get => _filteredItems;
            set
            {
                _filteredItems = value;
            }
        }

        private string DropdownStyle => isDropdownOpen ? "display: block;" : "display: none;";
        //private string DropdownStyle => isDropdownOpen ? "display: block;" : "display: none;";

        private void ToggleDropdown()
        {
            isDropdownOpen = !isDropdownOpen;
            

        }

        private Item SelectedItem => CustomerData.FirstOrDefault(item => item.IsSelected);
        private RouteItem SelectedRoute => RoutesData.FirstOrDefault(item => item.IsSelected);
       
        private void ToggleCheckboxCustomer(string itemId)
        {

            foreach (var item in CustomerData)
            {
                if (item.Id == itemId)
                {
                   
                    item.IsSelected = !item.IsSelected;

                    customerStoredId = item.IsSelected ? item.Id : null;
                    customerStoredName = item.IsSelected ? item.Name : null;
                }
                else
                {
                    item.IsSelected = false;
                    
                }
            }
        }
        public string selectedRouteName;
        private void routeToggleCheckbox(string itemId)
        {
            foreach (var item in RoutesData)
            {
                if (item.ID == itemId)
                {
                    item.IsSelected = !item.IsSelected;
                    if (item.IsSelected)
                    {
                        selectedRouteName = item.ID;
                    }
                    else
                    {
                        selectedRouteName = "";
                    }
                    break;
                }
                else
                {
                    item.IsSelected = false;
                }
            }

            // Update the filtered items based on the selected route
            FilteredCustomer = CustomerData.Where(item => item.RouteId == selectedRouteName).ToList();
        }


        public class FormData
        {
            [Required(ErrorMessage = "Order Type is required")]
            public string selectedOrderType { get; set; }

            [Required(ErrorMessage = "Route is required")]
            public string selectedRoute { get; set; }

            [Required(ErrorMessage = "Order Date is required")]
            public DateTime? selectedOrderDate { get; set; }

            [Required(ErrorMessage = "Delivery Date is required")]
            public DateTime? selectedDeliveryDate { get; set; }
        }



        private List<SelectCategoryType> selectCategoryTypes = new List<SelectCategoryType>
    {
        new SelectCategoryType { Id = "1", Name = "All Product", IsSelected=false },
       
        // Add more category types as neededselectCategories
    };

        // Child list
        private List<SelectCategory> selectCategories = new List<SelectCategory>
    {
        new SelectCategory { Id = "101", Name = "Butter", ParentId = "1",IsSelected=false },
        new SelectCategory { Id = "102", Name = "Cheese", ParentId = "1",IsSelected=false },
        new SelectCategory { Id = "103", Name = "IceCream", ParentId = "1",IsSelected=false },
        new SelectCategory { Id = "104", Name = "Milk", ParentId = "1",IsSelected=false }
        // Add more categories as needed
    };

        public List<SKUCategory> SKUCategoryList { get; set; } = new List<SKUCategory>
{
    // Four items for Butter (ParentId: 1)
    new SKUCategory { Id = "201", Name = "Butter SKU 1", ParentId = "101",IsPromo=true, IsMCL = false,IsChecked=false},
    new SKUCategory { Id = "202", Name = "Butter SKU 2", ParentId = "101" , IsPromo = false, IsMCL = false,IsChecked=false},
    new SKUCategory { Id = "203", Name = "Butter SKU 3", ParentId = "101" ,IsPromo=true, IsMCL = false ,IsChecked=false},
    new SKUCategory { Id = "204", Name = "Butter SKU 4", ParentId = "101" , IsPromo = false, IsMCL = false,IsChecked=false},

    // Four items for Cheese (ParentId: 1)
    new SKUCategory { Id = "205", Name = "Cheese SKU 1", ParentId = "102" ,IsPromo=true, IsMCL = false,IsChecked=false },
    new SKUCategory { Id = "206", Name = "Cheese SKU 2", ParentId = "102", IsPromo = false, IsMCL = false ,IsChecked=false},
    new SKUCategory { Id = "207", Name = "Cheese SKU 3", ParentId = "102", IsPromo = false, IsMCL = false ,IsChecked=false},
    new SKUCategory { Id = "208", Name = "Cheese SKU 4", ParentId = "102" ,IsPromo=true, IsMCL = false , IsChecked = false},

    // Four items for IceCream (ParentId: 2)
    new SKUCategory { Id = "209", Name = "IceCream SKU 1", ParentId = "103", IsPromo = false, IsMCL = false ,IsChecked=false},
    new SKUCategory { Id = "210", Name = "IceCream SKU 2", ParentId = "103" ,IsPromo=true, IsMCL = false ,IsChecked=false},
    new SKUCategory { Id = "211", Name = "IceCream SKU 3", ParentId = "103" ,IsPromo=true, IsMCL = false ,IsChecked=false},
    new SKUCategory { Id = "212", Name = "IceCream SKU 4", ParentId = "103", IsPromo = false, IsMCL = false,IsChecked=false },

    // Four items for Milk (ParentId: 2)
    new SKUCategory { Id = "213", Name = "Milk SKU 1", ParentId = "104" ,IsPromo=true, IsMCL = false,IsChecked=false },
    new SKUCategory { Id = "214", Name = "Milk SKU 2", ParentId = "104" ,IsPromo=true, IsMCL = false ,IsChecked=false},
    new SKUCategory { Id = "215", Name = "Milk SKU 3", ParentId = "104", IsPromo = false, IsMCL = false,IsChecked=false },
    new SKUCategory { Id = "216", Name = "Milk SKU 4", ParentId = "104", IsPromo = false, IsMCL = false ,IsChecked=false},
};

        private List<RouteItem> RoutesData = new List<RouteItem>
    {
        new RouteItem { ID="r1", Name = "OneRoute", IsSelected = false },
        new RouteItem { ID="r2", Name = "TwoRoute", IsSelected = false },
        new RouteItem { ID="r2", Name = "ThreeRoute", IsSelected = false },
        // Add more routes as needed
    };

    }

}
