using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Winit.Modules.Survey.Model.Interfaces
{
    public interface IStoreUserInfo
    {
         string StoreCode { get; set; }
         string StoreName { get; set; }
         string EmpCode { get; set; }
         string EmpName { get; set; }
         string Designation { get; set; }
         string SaleCategory { get; set; }
         int NoOfTimesVisited { get; set; }
        DateTime StartDate { get; set; }  // Added StartDate
        DateTime EndDate { get; set; }    // Added EndDate
        string LoginTime { get; set; }
        public string Locationvalue { get; set; }
        public string LocationCode { get; set; }
    }
}
