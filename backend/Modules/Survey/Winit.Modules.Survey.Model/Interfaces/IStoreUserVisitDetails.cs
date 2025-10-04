using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Winit.Modules.Survey.Model.Interfaces
{
    public interface IStoreUserVisitDetails: IStoreUserInfo
    {
      DateTime VisitDate {  get; set; }
      int DistanceVariance {  get; set; }
        TimeSpan StartTime {  get; set; }
        TimeSpan EndTime {  get; set; }
     string TimeSpent {  get; set; }
     DateTime VisitDateTime {  get; set; }
        string Status { get; set; }
        string LocationCode { get; set; }
        string Users { get; set; }

    }
}
