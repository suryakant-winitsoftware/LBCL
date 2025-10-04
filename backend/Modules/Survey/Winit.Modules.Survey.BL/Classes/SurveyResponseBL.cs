using Winit.Modules.Survey.DL.Interfaces;
using Winit.Modules.Survey.Model.Interfaces;
using Winit.Shared.Models.Common;
using Winit.Shared.Models.Enums;

namespace Winit.Modules.Survey.BL.Classes
{
    public class SurveyResponseBL : Interfaces.ISurveyResponseBL
    {
        protected readonly DL.Interfaces.ISurveyResponseDL _surveyResponseDL;
        public SurveyResponseBL(ISurveyResponseDL surveyResponseDL)
        {
            _surveyResponseDL = surveyResponseDL;
        }

        public async Task<PagedResponse<Winit.Modules.Survey.Model.Interfaces.ISurveyResponseModel>> GetAllSurveyResponse(List<SortCriteria> sortCriterias, int pageNumber,
        int pageSize, List<FilterCriteria> filterCriterias, bool isCountRequired)
        {
            return await _surveyResponseDL.GetAllSurveyResponse(sortCriterias, pageNumber, pageSize, filterCriterias, isCountRequired);
        }
        public async Task<Winit.Modules.Survey.Model.Interfaces.ISurveyResponseModel> GetSurveyResponseByUID(string UID)
        {
            return await _surveyResponseDL.GetSurveyResponseByUID(UID);
        }
     public async   Task<int> CreateSurveyResponse(Winit.Modules.Survey.Model.Interfaces.ISurveyResponseModel surveyResponseModel)
        {
            return await _surveyResponseDL.CreateSurveyResponse(surveyResponseModel);
        }
     public async   Task<int> UpdateSurveyResponse(Winit.Modules.Survey.Model.Interfaces.ISurveyResponseModel surveyResponseModel)
        {
            return await _surveyResponseDL.UpdateSurveyResponse(surveyResponseModel);
        }
        public async Task<PagedResponse<Winit.Modules.Survey.Model.Interfaces.IViewSurveyResponse>> GetViewSurveyResponse(List<SortCriteria> sortCriterias, int pageNumber,
        int pageSize, List<FilterCriteria> filterCriterias, bool isCountRequired)
        {
            return await _surveyResponseDL.GetViewSurveyResponse(sortCriterias, pageNumber, pageSize, filterCriterias, isCountRequired);
        }
        public async Task<ISurveyResponseViewDTO> ViewSurveyResponseByUID(string UID)
        {
            return await _surveyResponseDL.ViewSurveyResponseByUID(UID);
        }

        public async Task<ISurveySection> GetSurveySection(string UID)
        {
            return await _surveyResponseDL.GetSurveySection(UID);
        }

        public async Task<ISurveyResponseModel> GetSurveyResponseByUID(string sectionUID, string StoreHistoryUID, DateTime? submmitedDate)
        {
            return await _surveyResponseDL.GetSurveyResponseByUID(sectionUID, StoreHistoryUID, submmitedDate);
        }

        public async Task<List<ISurveyResponseModel>> GetSurveyResponse(string ActivityType, string LinkedItemUID)
        {
            return await _surveyResponseDL.GetSurveyResponse(ActivityType, LinkedItemUID);
        }
        public async Task<List<IViewSurveyResponseExport>> GetViewSurveyResponseForExport(List<FilterCriteria> filterCriterias)
        {
            return await _surveyResponseDL.GetViewSurveyResponseForExport(filterCriterias);
        }
        public async Task<int> TicketStatusUpdate(string uid, string status,string empUID)
        {
            return await _surveyResponseDL.TicketStatusUpdate(uid, status, empUID);
        }
        public async Task<PagedResponse<Winit.Modules.Survey.Model.Interfaces.IStoreQuestionFrequency>> GetStoreQuestionFrequencyDetails(List<SortCriteria> sortCriterias, int pageNumber,
        int pageSize, List<FilterCriteria> filterCriterias, bool isCountRequired)
        {
            return await _surveyResponseDL.GetStoreQuestionFrequencyDetails(sortCriterias, pageNumber, pageSize, filterCriterias, isCountRequired);
        }
        public async Task<int> CreateSurveyResponseList()
        {
            return await _surveyResponseDL.CreateSurveyResponseList();
        }

    }
}
