
using Winit.Modules.Survey.Model.Interfaces;
using Winit.Shared.Models.Common;
using Winit.Shared.Models.Enums;

namespace Winit.Modules.Survey.BL.Interfaces
{
    public interface ISurveyBL
    {
        Task<Winit.Modules.Survey.Model.Interfaces.ISurvey> GetSurveyByUID(string UID);
        Task<Winit.Modules.Survey.Model.Interfaces.ISurvey> GetSurveyByCode(string code);
        Task<int> CreateSurvey(Winit.Modules.Survey.Model.Interfaces.ISurvey survey);
        Task<int> UpdateSurvey(Winit.Modules.Survey.Model.Interfaces.ISurvey survey);
        Task<int> CUDSurvey(Winit.Modules.Survey.Model.Interfaces.ISurvey survey);
        Task<PagedResponse<Winit.Modules.Survey.Model.Interfaces.ISurvey>> GetAllSurveyDeatils(List<SortCriteria> sortCriterias, int pageNumber,
        int pageSize, List<FilterCriteria> filterCriterias, bool isCountRequired);
        Task<int> DeleteSurvey(string uID);

    }
}
