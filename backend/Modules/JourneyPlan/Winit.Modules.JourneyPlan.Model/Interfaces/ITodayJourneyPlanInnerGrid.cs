using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using Winit.Modules.Base.Model;

namespace Winit.Modules.JourneyPlan.Model.Interfaces
{
    public interface ITodayJourneyPlanInnerGrid
    {
        public string StoreNumber { get; set; }
        public int SeqNo { get; set; }
        public string StoreCode { get; set; }
        public string StoreUID { get; set; }
        public string StoreName { get; set; }
        public bool IsPlanned { get; set; }
        public string VisitStatus { get; set; }
        public TimeSpan VisitTime { get; set; }
     
      

    }
   
}
