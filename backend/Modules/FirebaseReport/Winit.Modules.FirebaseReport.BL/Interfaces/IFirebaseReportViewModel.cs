using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Winit.Modules.FirebaseReport.Models.Interfaces;

namespace Winit.Modules.FirebaseReport.BL.Interfaces
{
    public interface IFirebaseReportViewModel
    {
        List<IFirebaseReport> FirebaseDataReports { get; set; } 
        IFirebaseReport FirebaseDetails { get; set; }
        Task<int> RepostFirebaseLog(IFirebaseReport FirebaseReport);
        Task GetFirebaseDetails(string UID);
        Task LoadFirebaseData();
        Task LoadFirebaseDetails(string UID);
        Task ApplyFilter(Dictionary<string,string> keyValuePairs);
    }
}
