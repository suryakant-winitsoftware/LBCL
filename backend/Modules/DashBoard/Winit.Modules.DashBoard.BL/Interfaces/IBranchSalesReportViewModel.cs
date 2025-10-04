using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Winit.Modules.Base.BL.Interfaces;
using Winit.Modules.DashBoard.Model.Interfaces;

namespace Winit.Modules.DashBoard.BL.Interfaces
{
    public interface IBranchSalesReportViewModel : ITableGridViewModel
    {
        List<IBranchSalesReport> BranchSalesReport { get; set; }
        List<IBranchSalesReportAsmview> BranchSalesReportAsmviews { get; set; }
        List<IBranchSalesReportOrgview> BranchSalesReportOrgviews { get; set; }

        Task PopulateViewModel();
    }
}
