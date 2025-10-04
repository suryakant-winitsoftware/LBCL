using Microsoft.AspNetCore.Components;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Winit.UIComponents.Web.Paging
{
    public partial class Paging : ComponentBase
    {
        [Parameter] public int TotalItems { get; set; }
        [Parameter] public int PageSize { get; set; }
        [Parameter]
        public int CurrentPage
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
        [Parameter] public EventCallback<int> OnPageChange { get; set; }

        private int TotalPages => (int)Math.Ceiling(TotalItems / (double)PageSize);
        private int _currentPage; // Internal variable to manage current page
        private int StartPage => Math.Max(1, Math.Min(_currentPage - 4, TotalPages - 9));
        private int EndPage => Math.Min(StartPage + 9, TotalPages);


        protected override void OnInitialized()
        {
            _currentPage = CurrentPage; // Set internal current page on initialization
        }
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
    }
}
