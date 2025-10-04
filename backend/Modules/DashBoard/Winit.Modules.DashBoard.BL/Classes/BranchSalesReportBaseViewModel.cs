using DocumentFormat.OpenXml.Wordprocessing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Winit.Modules.DashBoard.BL.Interfaces;
using Winit.Modules.DashBoard.Model.Interfaces;
using Winit.Shared.Models.Common;
using Winit.Shared.Models.Enums;

namespace Winit.Modules.DashBoard.BL.Classes
{
    public abstract class BranchSalesReportBaseViewModel : IBranchSalesReportViewModel
    {
        public int TotalItems { get; set; }
        //public int PageSize { get; set; }
        //public int CurrentPage { get; set; }
        public PagingRequest PagingRequest { get; set; } = new PagingRequest()
        {
            FilterCriterias = [],
            SortCriterias = [],
        };
        public List<IBranchSalesReport> BranchSalesReport { get; set; } = [];
        public List<IBranchSalesReportAsmview> BranchSalesReportAsmviews { get; set; } = [];
        public List<IBranchSalesReportOrgview> BranchSalesReportOrgviews { get; set; } = [];
        public async Task PopulateViewModel()
        {
            PagingRequest.PageNumber = 1;
            PagingRequest.PageSize = 20;
            await GetBranchSalesReport();
        }
        public async Task OnFilterApply(IDictionary<string, string> filterCriteria)
        {
            PagingRequest.FilterCriterias!.Clear();
            if (filterCriteria != null)
            {
                foreach (var filter in filterCriteria)
                {
                    PagingRequest.FilterCriterias.Add(new(filter.Key, filter.Value, FilterType.Equal));
                    switch (filter.Key)
                    {

                        case "Example1":
                            break;
                    }

                }
            }
            throw new NotImplementedException();
        }
        public async Task OnSort(SortCriteria sortCriteria)
        {
            throw new NotImplementedException();
        }
        public async Task OnPageChange(int pageNumber)
        {
            PagingRequest.PageNumber = pageNumber;
            await GetBranchSalesReport();
        }


        protected abstract Task GetBranchSalesReport();

    }
}
