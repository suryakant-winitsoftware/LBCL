using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Winit.Shared.Models.Enums;
using Winit.Shared.Models.Common;
using Winit.Modules.CaptureCompetitor.Model.Classes;
using Winit.Modules.CaptureCompetitor.Model.Interfaces;


namespace Winit.Modules.CaptureCompetitor.BL.Interfaces
{
    public interface ICaptureCompetitorBL
    {
        Task<PagedResponse<Winit.Modules.CaptureCompetitor.Model.Interfaces.ICaptureCompetitor>> GetCaptureCompetitorDetails(List<SortCriteria> sortCriterias, int pageNumber,
       int pageSize, List<FilterCriteria> filterCriterias, bool isCountRequired);
        Task<Winit.Modules.CaptureCompetitor.Model.Interfaces.ICaptureCompetitor> GetCaptureCompetitorDetailsByUID(string UID);
        Task<int> CreateCaptureCompetitor(Winit.Modules.CaptureCompetitor.Model.Interfaces.ICaptureCompetitor captureCompetitor);
        Task<int> UpdateCaptureCompetitor(Winit.Modules.CaptureCompetitor.Model.Interfaces.ICaptureCompetitor captureCompetitor);
        Task<int> DeleteCaptureCompetitor(string UID);

    }
}
