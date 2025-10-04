using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Winit.Modules.NewsActivity.BL.Interfaces;
using Winit.Modules.NewsActivity.Models.Constants;
using Winit.Modules.NewsActivity.Models.Interfaces;
using Winit.Modules.Scheme.Model.Classes;
using Winit.Modules.Scheme.Model.Constants;
using Winit.Shared.CommonUtilities.Common;
using Winit.Shared.Models.Common;
using Winit.Shared.Models.Enums;

namespace Winit.Modules.NewsActivity.BL.Classes
{
    public abstract class ManageNewsActivityBaseViewModel : IManageNewsActivityViewModel
    {
        public List<ISelectionItem> TabItems { get; set; } = [];
        public ISelectionItem SelectedTab { get; set; }
        public List<INewsActivity> NewsActivities { get; set; } = [];
        public int TotalItems { get; set; }
        public int PageSize { get; set; } = 50;
        public int CurrentPage { get; set; } = 1;
        protected PagingRequest PagingRequest = new();

        protected FilterCriteria DefaultFilter = new(name: nameof(INewsActivity.ActivityType), NewsActivityConstants.news, type: FilterType.Equal);
        protected SortCriteria DefaultSortCriteria = new SortCriteria(nameof(INewsActivity.ModifiedTime),SortDirection.Desc);
        protected bool IsFilesysNeeded { get; set; }
        public async Task PopulateviewModel(bool isFilesysNeeded = false)
        {
            IsFilesysNeeded = isFilesysNeeded;
            TabItems.Clear();
            SelectedTab = new SelectionItem() { Code = NewsActivityConstants.news, Label = "News", IsSelected = true };
            TabItems.Add(SelectedTab);
            TabItems.Add(new SelectionItem() { Code = NewsActivityConstants.advertisement, Label = "Advertisement" });
            TabItems.Add(new SelectionItem() { Code = NewsActivityConstants.businesscommunication, Label = "Business Communication" });

            PagingRequest.FilterCriterias = [DefaultFilter];
            PagingRequest.SortCriterias = [DefaultSortCriteria];
            PagingRequest.PageSize = PageSize;
            PagingRequest.PageNumber = 1;
            PagingRequest.IsCountRequired = true;
            await GetAllNewsActivity();
        }
        public async Task OnTabSelect(ISelectionItem selectedItem)
        {
            SelectedTab = selectedItem;
            //PagingRequest.FilterCriterias.Clear();
            TabItems.ForEach(p =>
            {
                p.IsSelected = false;
            });
            selectedItem.IsSelected = true;
            DefaultFilter.Value = selectedItem.Code;
            PagingRequest.FilterCriterias!.ForEach(p =>
            {
                if (p.Name.Equals(DefaultFilter.Name))
                {
                    p.Value = DefaultFilter.Value;
                }
            });
            PagingRequest.PageNumber = 1;
            await GetAllNewsActivity();
        }
        public async Task OnPageChange(int pageNumber)
        {
            PagingRequest.PageNumber =CurrentPage= pageNumber;
            await GetAllNewsActivity();
        }
        public async Task OnFilterApply(Dictionary<string, string> filterCriteria)
        {
            PagingRequest.FilterCriterias!.Clear();
            PagingRequest.FilterCriterias.Add(DefaultFilter);
            if (filterCriteria is not null)
            {
                foreach (var item in filterCriteria)
                {
                    if (item.Key == SchemeConstants.FromDate)
                    {
                        PagingRequest.FilterCriterias.Add(
                            new FilterCriteria(nameof(SellInSchemeHeader.ValidFrom),
                            string.IsNullOrEmpty(item.Value) || string.IsNullOrWhiteSpace(item.Value) ? CommonFunctions.GetFirstDayOfMonth(DateTime.UtcNow) : CommonFunctions.GetDate(item.Value),
                            FilterType.GreaterThanOrEqual));
                    }
                    else if (item.Key == SchemeConstants.ToDate && !string.IsNullOrWhiteSpace(item.Value))
                    {
                        PagingRequest.FilterCriterias.Add(
                            new FilterCriteria(nameof(SellInSchemeHeader.ValidUpTo), CommonFunctions.GetDate(item.Value),
                           FilterType.LessThanOrEqual));
                    }
                    else
                    {
                        if (!string.IsNullOrEmpty(item.Value))
                        {
                            PagingRequest.FilterCriterias.Add(new FilterCriteria(item.Key, item.Value, FilterType.Equal));
                        }
                    }
                }

            }
            PagingRequest.PageNumber = 1;
            await GetAllNewsActivity();
        }
        protected abstract Task GetAllNewsActivity();
    }
}
