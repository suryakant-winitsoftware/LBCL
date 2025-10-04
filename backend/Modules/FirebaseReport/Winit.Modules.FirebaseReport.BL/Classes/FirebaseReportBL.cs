using Winit.Modules.FirebaseReport.BL.Interfaces;
using Winit.Modules.FirebaseReport.Models.Interfaces;
using Winit.Shared.Models.Common;
using Winit.Shared.Models.Enums;


namespace Winit.Modules.FirebaseReport.BL.Classes
{
    public class FirebaseReportBL: IFirebaseReportBL
    {
        protected readonly DL.Interfaces.IFirebaseReportDL _FirebaseReportDL = null;
        public FirebaseReportBL(DL.Interfaces.IFirebaseReportDL FirebaseReportDL)
        {
            _FirebaseReportDL = FirebaseReportDL;
        }
        public async Task<PagedResponse<IFirebaseReport>> SelectAllFirebaseReportDetails(List<SortCriteria> sortCriterias, int pageNumber,
            int pageSize, List<FilterCriteria> filterCriterias, bool isCountRequired)
        {
            return await _FirebaseReportDL.SelectAllFirebaseReportDetails(sortCriterias, pageNumber, pageSize, filterCriterias, isCountRequired);
        }

        public async Task<IFirebaseReport> SelectFirebaseDetailsData(string UID)
        {
            return await _FirebaseReportDL.SelectFirebaseDetailsData(UID);
        }

        public async Task<IFirebaseReport> GetFirebaseReportByUID(string UID)
        {
            return await _FirebaseReportDL.GetFirebaseReportByUID(UID);
        }

        public async Task<IEnumerable<string>> BindFilterValues(string type)
        {
            return await _FirebaseReportDL.BindFilterValues(type);
        }
    }
}
