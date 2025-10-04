using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Winit.Modules.CaptureCompetitor.DL.Interfaces;
using Winit.Shared.Models.Enums;
using Winit.Shared.Models.Common;
using Winit.Modules.CaptureCompetitor.Model.Classes;
using Winit.Modules.CaptureCompetitor.Model.Interfaces;

namespace Winit.Modules.CaptureCompetitor.BL.Classes
{
    public class CaptureCompetitorBL : Interfaces.ICaptureCompetitorBL
    {
        protected readonly DL.Interfaces.ICaptureCompetitorDL _captureCompetitorDL;
        public CaptureCompetitorBL(ICaptureCompetitorDL captureCompetitorDL)
        {
            _captureCompetitorDL = captureCompetitorDL;
        }
        public async Task<PagedResponse<Winit.Modules.CaptureCompetitor.Model.Interfaces.ICaptureCompetitor>> GetCaptureCompetitorDetails(List<SortCriteria> sortCriterias, int pageNumber,
        int pageSize, List<FilterCriteria> filterCriterias, bool isCountRequired)
        {
            return await _captureCompetitorDL.GetCaptureCompetitorDetails(sortCriterias, pageNumber, pageSize, filterCriterias, isCountRequired);
        }

        public async Task<Winit.Modules.CaptureCompetitor.Model.Interfaces.ICaptureCompetitor> GetCaptureCompetitorDetailsByUID(string UID)
        {
            return await _captureCompetitorDL.GetCaptureCompetitorDetailsByUID(UID);
        }
        public async Task<int> CreateCaptureCompetitor(Winit.Modules.CaptureCompetitor.Model.Interfaces.ICaptureCompetitor captureCompetitor)
        {
            return await _captureCompetitorDL.CreateCaptureCompetitor(captureCompetitor);
        }
        public async Task<int> UpdateCaptureCompetitor(Winit.Modules.CaptureCompetitor.Model.Interfaces.ICaptureCompetitor captureCompetitor)
        {
            return await _captureCompetitorDL.UpdateCaptureCompetitor(captureCompetitor);
        }
        public async Task<int> DeleteCaptureCompetitor(string UID)
        {
            return await _captureCompetitorDL.DeleteCaptureCompetitor(UID);
        }

    }
}
