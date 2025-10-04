using Microsoft.AspNetCore.Components;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Winit.Shared.Models.Enums;
using Winit.Shared.Models.Common;

namespace Winit.UIComponents.Web.Table
{
    public partial class DivGridView : ComponentBase
    {
        [Parameter] public IList DataSource { get; set; }
        [Parameter] public List<DataGridColumn> Columns { get; set; }
        [Parameter] public EventCallback<SortCriteria> OnSort { get; set; }

        //Created by Selva
        [Parameter] public bool IsFirstColumnCheckbox { get; set; }
        [Parameter] public EventCallback<HashSet<object>> AfterCheckBoxSelection { get; set; }
        private HashSet<object> SelectedItems = new HashSet<object>();
        private bool SelectAllChecked = false;
        //

        private string currentSortField;

        private bool isAscending = true;
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
                SortCriteria sortCriteria = new SortCriteria (currentSortField, isAscending ? SortDirection.Asc : SortDirection.Desc );
                OnSort.InvokeAsync(sortCriteria);
            }
        }
        //Created by Selva
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
        //Created by Selva
        private void ToggleAllCheckboxes()
        {
            SelectAllChecked = !SelectAllChecked;
            SelectedItems.Clear();
            // Logic to toggle all checkboxes in the first column
            // For example:
            foreach (var item in DataSource)
            {
                // Assuming SelectedItems is a collection storing the selected items
                if (SelectAllChecked)
                {
                    SelectedItems.Add(item);
                }
                else
                {
                    SelectedItems.Remove(item);
                }
            }
            AfterCheckBoxSelection.InvokeAsync(SelectedItems);
        }
    }
}
