using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Winit.Modules.DashBoard.Model.Interfaces;

namespace Winit.Modules.DashBoard.Model.Classes
{
    public class BranchSalesReportOrgview : IBranchSalesReportOrgview
    {
        public string OrgCode {  get; set; }
        public string OrgName {  get; set; }
        public decimal TotalUnits {  get; set; }
        public decimal TotalSales {  get; set; }
    }
}
