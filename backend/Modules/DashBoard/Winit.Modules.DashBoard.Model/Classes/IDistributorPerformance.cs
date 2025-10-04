using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Winit.Modules.DashBoard.Model.Interfaces;

namespace Winit.Modules.DashBoard.Model.Classes
{
    public class DistributorPerformance:IDistributorPerformance
    {
        public int Month { get; set; }
        public int Quarter { get; set; }
        public decimal TargetValue { get; set; }
        public string DistributorUid { get; set; }
        public decimal AchievedValue { get; set; }
        public  decimal LastYearAchievedValue { get; set; }
        public decimal AchievementPercentage { get; set; }
        public decimal GrowthPercentage { get; set; }
       public int Year { get; set; }

    }
}
