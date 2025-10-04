using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Winit.Modules.DashBoard.Model.Interfaces
{
    public interface IBranchSalesReportAsmview
    {
        string ASMCode { get; set; }
        string ASMName { get; set; }
        decimal TotalUnits { get; set; }
        decimal TotalSales { get; set; }
    }
}
