using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Winit.Modules.DashBoard.Model.Interfaces
{
    public interface IBranchSalesReportOrgview
    {
        string OrgCode { get; set; }
        string OrgName { get; set; }
        decimal TotalUnits { get; set; }
        decimal TotalSales { get; set; }
    }
}
