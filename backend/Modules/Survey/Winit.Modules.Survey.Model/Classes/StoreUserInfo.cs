using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Winit.Modules.Survey.Model.Interfaces;

namespace Winit.Modules.Survey.Model.Classes
{
    public class StoreUserInfo : IStoreUserInfo
    {
      public string StoreCode { get; set; }
      public string StoreName { get; set; }
      public string EmpCode { get; set; }
      public string EmpName { get; set; }
      public string Designation { get; set; }
      public string SaleCategory { get; set; }
      public int NoOfTimesVisited { get; set; }
        public DateTime StartDate { get; set; }  // Implemented StartDate
        public DateTime EndDate { get; set; }    // Implemented EndDate
        public string  LoginTime { get; set; }
        public string Locationvalue { get; set; }
        public string LocationCode { get; set; }

    }
}
