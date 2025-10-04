using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Winit.Modules.DashBoard.Model.Interfaces;

namespace Winit.Modules.DashBoard.Model.Classes
{
    public class BranchSalesReport: IBranchSalesReport
    {

        public string BranchCode { get; set; }
        public string BranchName { get; set; }
        public int PeriodYear { get; set; }
        public int TotalUnits { get; set; }
        public decimal TotalSales { get; set; }
       public string ASMCode { get; set; }
       public string ASMName { get; set; }
       public string OrgCode { get; set; }
       public string OrgName { get; set; }

    }
}
