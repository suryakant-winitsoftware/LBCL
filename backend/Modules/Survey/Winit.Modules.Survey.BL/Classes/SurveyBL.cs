using Winit.Modules.Survey.DL.Interfaces;
using Winit.Modules.Survey.Model.Interfaces;
using Winit.Shared.Models.Common;
using Winit.Shared.Models.Enums;

namespace Winit.Modules.Survey.BL.Classes
{
    public class SurveyBL : Interfaces.ISurveyBL
    {
        protected readonly DL.Interfaces.ISurveyDL _surveyDL;
        public SurveyBL(ISurveyDL surveyDL)
        {
            _surveyDL = surveyDL;
        }

     public async   Task<Winit.Modules.Survey.Model.Interfaces.ISurvey> GetSurveyByUID(string UID)
        {
            return await _surveyDL.GetSurveyByUID(UID);
        }
       public async Task<Winit.Modules.Survey.Model.Interfaces.ISurvey> GetSurveyByCode(string code)
        {
            return await _surveyDL.GetSurveyByCode(code);

        }

        public async Task<int> CreateSurvey(Winit.Modules.Survey.Model.Interfaces.ISurvey survey)
        {
            return await _surveyDL.CreateSurvey(survey);
        }
      public async  Task<int> UpdateSurvey(Winit.Modules.Survey.Model.Interfaces.ISurvey survey)
        {
            return await _surveyDL.UpdateSurvey(survey);
        }
        public async Task<int> CUDSurvey(Winit.Modules.Survey.Model.Interfaces.ISurvey survey)
        {
            return await _surveyDL.CUDSurvey(survey);
        }
        public async  Task<PagedResponse<Winit.Modules.Survey.Model.Interfaces.ISurvey>> GetAllSurveyDeatils(List<SortCriteria> sortCriterias, int pageNumber,
        int pageSize, List<FilterCriteria> filterCriterias, bool isCountRequired)
        {
            return await _surveyDL.GetAllSurveyDeatils(sortCriterias, pageNumber, pageSize, filterCriterias, isCountRequired);
        }
       public async Task<int> DeleteSurvey(string uID)
        {
            return await _surveyDL.DeleteSurvey(uID);
        }


    }
}
