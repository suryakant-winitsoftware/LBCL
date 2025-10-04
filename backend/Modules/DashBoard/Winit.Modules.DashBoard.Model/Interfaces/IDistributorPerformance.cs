using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Winit.Modules.DashBoard.Model.Interfaces
{
    public interface IDistributorPerformance
    {
        string DistributorUid { get; set; }
        int Year { get; set; }
        int Month { get; set; }
        int Quarter { get; set; }
        decimal TargetValue { get; set; }
        decimal AchievedValue { get; set; }
        decimal LastYearAchievedValue { get; set; }
        decimal AchievementPercentage { get; set; }
        decimal GrowthPercentage { get; set; }

    }
}
