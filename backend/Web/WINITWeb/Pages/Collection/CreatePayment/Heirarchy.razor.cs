using Microsoft.AspNetCore.Components;
using NPOI.OpenXmlFormats.Vml.Office;
using Practice;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Resources;
using Winit.Modules.Base.BL.Helper.Classes;
using Winit.Modules.Base.BL.Helper.Interfaces;
using Winit.Modules.CollectionModule.Model.Classes;
using Winit.Modules.SKU.Model.Classes;
using Winit.Modules.SKU.Model.Interfaces;
using Winit.Shared.Models.Common;
using Winit.Shared.Models.Enums;
using Winit.UIComponents.Common.Language;
using Winit.UIComponents.Web.SalesManagement.PriceManagement;

using WinIt.Pages.Base;

namespace WinIt.Pages.Collection.CreatePayment
{
    public partial class Heirarchy : BaseComponentBase
    {
        //reflection
        private Dictionary<object, bool> rowVisibilityStates = new Dictionary<object, bool>();
        [Parameter] public IList DataSource { get; set; }
        [Parameter] public IList DataSource2 { get; set; }
        [Parameter] public List<DataGridColumn> Columns { get; set; }
        [Parameter] public List<DataGridColumn> Columns2 { get; set; }
        [Parameter] public EventCallback<SortCriteria> OnSort { get; set; }
        [Parameter] public bool IsFirstColumnCheckbox { get; set; }
        [Parameter] public bool _showChild { get; set; } = false;
        [Parameter] public string Textvalue { get; set; } = "";
        [Parameter] public static bool flag { get; set; } = false;
        private decimal textBoxValue { get; set; } = 0;
        [Parameter] public Receipt receipt { get; set; }

        [Parameter] public EventCallback<HashSet<object>> AfterCheckBoxSelection { get; set; }
        //Sorting
        private string currentSortField;
        private bool isAscending = true;
        //Checkbox
        private HashSet<object> SelectedItems = new HashSet<object>();
        [Parameter] public HashSet<object> SelectedItemsStatic { get; set; } = new HashSet<object>();
        private static List<object> SelectedList = new List<object>();
        private bool SelectAllChecked = false;
        [Parameter] public EventCallback OnSelect { get; set; }
        [Parameter] public EventCallback<HashSet<object>> OnTextChanged { get; set; }



        protected override void OnInitialized()
        {
            base.OnInitialized();
            LoadResources(null, _languageService.SelectedCulture);
        }


        private void SortColumn(DataGridColumn column)
        {
            if (column.IsSortable)
            {
                if (currentSortField == column.SortField)
                {
                    isAscending = !isAscending;
                }
                else
                {
                    currentSortField = column.SortField;
                    isAscending = true;
                }
                SortCriteria sortCriteria = new SortCriteria(currentSortField, isAscending ? SortDirection.Asc : SortDirection.Desc);
                OnSort.InvokeAsync(sortCriteria);
            }
        }
        private void ToggleSelection(object item)
        {
            if (SelectedItems.Contains(item))
            {
                SelectedItems.Remove(item);
            }
            else
            {
                SelectedItems.Add(item);
            }
            AfterCheckBoxSelection.InvokeAsync(SelectedItems);
        }

        //here i am using states for each row because i am using a single variable for textbox in each row of child table
        private void ToggleDetails(object item)
        {
            if (rowVisibilityStates.ContainsKey(item))
            {
                rowVisibilityStates[item] = !rowVisibilityStates[item];
            }
            else
            {
                rowVisibilityStates.Add(item, true);
            }
            OnSelect.InvokeAsync();
        }

        private void ToggleAllCheckboxes()
        {
            SelectAllChecked = !SelectAllChecked;
            SelectedItems.Clear();
            // Logic to toggle all checkboxes in the first column
            // For example:
            foreach (var item in DataSource2)
            {
                // Assuming SelectedItems is a collection storing the selected items
                if (SelectAllChecked)
                {
                    SetPropertyValue(item, "EnteredAmount");
                    SelectedItems.Add(item);
                }
                else
                {
                    SelectedItems.Remove(item);
                }
            }
            AfterCheckBoxSelection.InvokeAsync(SelectedItems);
        }

        //this gets the property value we give 
        private object GetPropertyValue(object obj, string propertyName)
        {
            var property = obj.GetType().GetProperty(propertyName);
            if (property != null)
            {
                try
                {
                    return Convert.ChangeType(property.GetValue(obj), typeof(object));
                }
                catch (Exception)
                {
                    // Handle the type conversion error, e.g., log or return a default value
                    return null;
                }
            }


            return null;
        }

        private object SetPropertyValue(object obj, string propertyName)
        {
            var property = obj.GetType().GetProperty(propertyName);
            if (property == null)
            {
                return property.GetValue(obj);
            }

            return null;
        }

        //whenever value changes in textbox this method gets hit and sets a value to the property we give
        private void SetPropertyValue(object obj, string propertyName, object value)
        {
            var property = obj.GetType().GetProperty(propertyName);
            if (property != null && property.CanWrite)
            {
                if (property.PropertyType == typeof(decimal))
                {
                    decimal decimalValue;
                    if (decimal.TryParse(value.ToString(), out decimalValue))
                    {
                        property.SetValue(obj, decimalValue);
                    }
                    else
                    {
                        // Handle the case where the conversion fails
                        // For example, set a default value or log an error
                    }
                }
                else
                {
                    // For other types, directly set the value
                    property.SetValue(obj, value);
                }
            }
            StateHasChanged();
            OnTextChanged.InvokeAsync(SelectedItems);
        }
        public bool GetCall(decimal CashAmount, bool condition)
        {
            SelectedItemsStatic = new HashSet<object>();
            HashSet<AccPayable> elementalHashSet = new HashSet<AccPayable>(DataSource2.OfType<AccPayable>());
            List<AccPayable> selectedItems = elementalHashSet.ToList();
            selectedItems = selectedItems.OrderBy(r => r.TransactionDate).ToList();
            selectedItems.RemoveAll(r => r.ReferenceNumber.Contains("CREDIT"));
            selectedItems.ForEach(item => item.EnteredAmount = 0);
            decimal comparer = 0;
            decimal Setcom = 0;
            decimal Setcas = 0;
            decimal cashamt = 0;
            if (condition)
            {
                flag = true;
            }
            else
            {
                flag = false;
            }
            foreach (var list in selectedItems)
            {
                if (flag)
                {
                    comparer = comparer == 0 ? list.BalanceAmount : comparer;
                    cashamt = cashamt == 0 ? CashAmount : cashamt;
                    if (cashamt > comparer)
                    {
                        cashamt = cashamt - comparer;
                        Setcom = comparer * receipt._collection.Rate;
                        comparer = 0;
                    }
                    else
                    {
                        comparer = comparer - cashamt;
                        Setcas = cashamt * receipt._collection.Rate;
                        cashamt = 0;
                    }
                    if (cashamt == 0)
                    {
                        SetPropertyValue(list, "EnteredAmount", Setcas);
                        SelectedItemsStatic.Add(list);
                        StateHasChanged();
                        return true;
                    }
                    else
                    {
                        SetPropertyValue(list, "EnteredAmount", Setcom);
                        SelectedItemsStatic.Add(list);
                    }
                }
                else
                {
                    SelectedItemsStatic = new HashSet<object>();
                    SetPropertyValue(list, "EnteredAmount", 0);
                }
            }
            StateHasChanged();
            return true;
        }
        private string GetCheckboxClass()
        {
            // Use a conditional expression to determine the class based on the 'flag' value
            if (flag)
            {
                return "light-blue-checkbox";
            }
            return "";
        }

        public void FlagFalse()
        {
            flag = false;
            StateHasChanged();
        }


    }
}
