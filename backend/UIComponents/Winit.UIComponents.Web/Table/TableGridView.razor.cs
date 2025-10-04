using Microsoft.AspNetCore.Components;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Winit.Shared.Models.Enums;
using Winit.Shared.Models.Common;

namespace Winit.UIComponents.Web.Table
{
    public partial class TableGridView : ComponentBase
    {
        [Parameter] public IList DataSource { get; set; }
        [Parameter] public List<DataGridColumn> Columns { get; set; }
        [Parameter] public EventCallback<SortCriteria> OnSort { get; set; }
        [Parameter] public bool IsFirstColumnCheckbox { get; set; }
        [Parameter] public EventCallback<HashSet<object>> AfterCheckBoxSelection { get; set; }
        [Parameter]
        public int PageNumber

        {
            get
            {
                return _currentPage;
            }
            set
            {
                _currentPage = value;
            }
        }
        [Parameter] public int PageSize { get; set; }
        [Parameter] public int TotalItemsCount { get; set; }
        [Parameter] public bool ShowRecordsCount { get; set; }
        [Parameter] public bool IsPaginationRequired { get; set; }

        
        [Parameter] public EventCallback<int> OnPageChange { get; set; }

        private int TotalPages => (int)Math.Ceiling(TotalItemsCount / (double)PageSize);
        private int _currentPage; // Internal variable to manage current page
        private int StartPage => Math.Max(1, Math.Min(_currentPage - 4, TotalPages - 9));
        private int EndPage => Math.Min(StartPage + 9, TotalPages);


        private bool IsFirstPage => _currentPage == 1;
        private bool IsLastPage => _currentPage == TotalPages;
        private async void FirstPage() => await ChangePage(1);
        private async void LastPage() => await ChangePage(TotalPages);
        private async void NextPage() => await ChangePage(_currentPage + 1);
        private async void PreviousPage() => await ChangePage(_currentPage - 1);
        private async Task ChangePage(int pageNumber)
        {
            if (pageNumber >= 1 && pageNumber <= TotalPages && pageNumber != _currentPage)
            {
                _currentPage = pageNumber;
                await OnPageChange.InvokeAsync(pageNumber);
            }
        }
        private bool IsShowImageInPopup;
        private string? ImageURL=null;
        //Sorting
        private string currentSortField;
        private bool isAscending = true;
        //Checkbox
        private HashSet<object> SelectedItems = new HashSet<object>();
        private bool SelectAllChecked = false;
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
        private string ViewingMessage
        {

            get
            {
                int startRecord = 0;
                int endRecord = 0;
                if (TotalItemsCount == 0)
                {
                    return $"You are viewing {startRecord}-{endRecord} out of {TotalItemsCount}";
                }
                else
                {
                    startRecord = ((PageNumber - 1) * PageSize) + 1;
                    endRecord = Math.Min(PageNumber * PageSize, TotalItemsCount);
                    return $"You are viewing {startRecord}-{endRecord} out of {TotalItemsCount}";
                }
            }

        }
    }
}
