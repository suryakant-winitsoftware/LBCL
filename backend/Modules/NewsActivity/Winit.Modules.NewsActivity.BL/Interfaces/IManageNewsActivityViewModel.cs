using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Winit.Modules.NewsActivity.Models.Interfaces;
using Winit.Shared.Models.Common;

namespace Winit.Modules.NewsActivity.BL.Interfaces
{
    public interface IManageNewsActivityViewModel
    {
        ISelectionItem SelectedTab { get; set; }
        List<ISelectionItem> TabItems { get; set; }
        List<INewsActivity> NewsActivities { get; set; }
        int TotalItems { get; set; }
        int PageSize { get; set; }
        int CurrentPage { get; set; }
        Task PopulateviewModel(bool isFilesysNeeded = false);
        Task OnFilterApply(Dictionary<string, string> filterCriteria);
        Task OnPageChange(int pageNumber);
        Task OnTabSelect(ISelectionItem selectedItem);
    }
}
