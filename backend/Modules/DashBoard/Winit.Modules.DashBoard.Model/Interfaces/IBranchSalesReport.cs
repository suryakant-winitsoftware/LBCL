using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Winit.Modules.DashBoard.Model.Interfaces
{
    public interface IBranchSalesReport
    {
        string BranchCode { get; set; }
        string BranchName { get; set; }
        int PeriodYear { get; set; }
        int TotalUnits { get; set; }
        decimal TotalSales { get; set; }
        string ASMCode { get; set; }
        string ASMName { get; set; }
        string OrgCode { get; set; }
        string OrgName { get; set; }
    }
}
