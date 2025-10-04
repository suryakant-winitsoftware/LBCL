using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Winit.Shared.Models.Common;
using Winit.Shared.Models.Enums;
using Winit.Modules.FirebaseReport.Models.Interfaces;

namespace Winit.Modules.FirebaseReport.DL.Interfaces
{
    public interface IFirebaseReportDL
    {
        Task<PagedResponse<IFirebaseReport>> SelectAllFirebaseReportDetails(List<SortCriteria> sortCriterias, int pageNumber,
         int pageSize, List<FilterCriteria> filterCriterias, bool isCountRequired);
        Task<IFirebaseReport> SelectFirebaseDetailsData(string UID);
        Task<IFirebaseReport> GetFirebaseReportByUID(string UID);
        Task<IEnumerable<string>> BindFilterValues(string type);
    }
}
