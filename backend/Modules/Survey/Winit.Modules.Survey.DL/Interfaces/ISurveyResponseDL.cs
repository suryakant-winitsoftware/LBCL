
using Winit.Modules.Survey.Model.Interfaces;
using Winit.Shared.Models.Common;
using Winit.Shared.Models.Enums;

namespace Winit.Modules.Survey.DL.Interfaces
{
    public interface ISurveyResponseDL
    {

        Task<Winit.Modules.Survey.Model.Interfaces.ISurveyResponseModel> GetSurveyResponseByUID(string UID);
        Task<int> CreateSurveyResponse(Winit.Modules.Survey.Model.Interfaces.ISurveyResponseModel surveyResponseModel);
        Task<int> UpdateSurveyResponse(Winit.Modules.Survey.Model.Interfaces.ISurveyResponseModel surveyResponseModel);
        Task<PagedResponse<Winit.Modules.Survey.Model.Interfaces.ISurveyResponseModel>> GetAllSurveyResponse(List<SortCriteria> sortCriterias, int pageNumber,
        int pageSize, List<FilterCriteria> filterCriterias, bool isCountRequired);
        Task<PagedResponse<Winit.Modules.Survey.Model.Interfaces.IViewSurveyResponse>> GetViewSurveyResponse(List<SortCriteria> sortCriterias, int pageNumber,
       int pageSize, List<FilterCriteria> filterCriterias, bool isCountRequired);
        Task<ISurveyResponseViewDTO> ViewSurveyResponseByUID(string UID);
        Task<ISurveySection> GetSurveySection(string UID);
        Task<Winit.Modules.Survey.Model.Interfaces.ISurveyResponseModel> GetSurveyResponseByUID(string sectionUID, string StoreHistoryUID, DateTime? submmitedDate);
        Task<List<Winit.Modules.Survey.Model.Interfaces.ISurveyResponseModel>> GetSurveyResponse(string ActivityType, string LinkedItemUID);
        Task<List<IViewSurveyResponseExport>> GetViewSurveyResponseForExport(List<FilterCriteria> filterCriterias);
        Task<int> TicketStatusUpdate(string uid, string status,string empUID);
        Task<PagedResponse<Winit.Modules.Survey.Model.Interfaces.IStoreQuestionFrequency>> GetStoreQuestionFrequencyDetails(List<SortCriteria> sortCriterias, int pageNumber,
      int pageSize, List<FilterCriteria> filterCriterias, bool isCountRequired);
        Task<int> CreateSurveyResponseList();

    }
}
