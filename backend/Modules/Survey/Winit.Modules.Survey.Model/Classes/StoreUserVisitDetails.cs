using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Winit.Modules.Survey.Model.Interfaces;

namespace Winit.Modules.Survey.Model.Classes
{
    public class StoreUserVisitDetails : StoreUserInfo, IStoreUserVisitDetails
    {
      public DateTime VisitDate { get; set; }
      public int DistanceVariance { get; set; }
      public TimeSpan StartTime { get; set; }
      public TimeSpan EndTime { get; set; }
      public string TimeSpent { get; set; }
      public  DateTime VisitDateTime {  get; set; }
        public string Status { get; set; }
        public string LocationCode { get; set; }
        public string Users { get; set; }
    }
}
