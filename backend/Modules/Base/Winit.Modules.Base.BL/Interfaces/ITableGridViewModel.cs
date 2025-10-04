using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Winit.Shared.Models.Common;
using Winit.Shared.Models.Enums;

namespace Winit.Modules.Base.BL.Interfaces
{
    public interface ITableGridViewModel
    {
        int TotalItems { get; set; }
        PagingRequest PagingRequest { get; set; }
        Task OnFilterApply(IDictionary<string, string> filterCriteria)
        {
            if (filterCriteria != null)
            {
                PagingRequest=new PagingRequest();
                foreach (var filter in filterCriteria)
                {
                    switch (filter.Key)
                    {

                        case "Example1":
                            break;
                    }
                }
            }
            return Task.CompletedTask;
        }
        Task OnSort(SortCriteria sortCriteria)
        {
            return Task.CompletedTask;
        }
        Task OnPageChange(int pageNumber)
        {
            return Task.CompletedTask;
        }
    }
}
