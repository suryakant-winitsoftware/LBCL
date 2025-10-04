using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Winit.Modules.FileSys.Model.Interfaces;
using Winit.Modules.Store.Model.Interfaces;
using Winit.Modules.Survey.Model.Interfaces;

namespace Winit.Modules.Survey.BL.Interfaces
{
    public interface ISurveyViewModel
    {
        string FolderPathVidoes { get; set; }
        string FolderPathImages { get; set; }
        ISurveySection SurveySection { get; set; }
        public Dictionary<string, ISurveyResponse> Responses { get; set; }
        public List<IServeyQuestions> Questions { get; set; }
        List<ISurveyResponseModel> SurveyResponseModels { get; set; }
        List<IFileSys> ImageFileSysList { get; set; }
        Task GetSurveySection(string UID);
        Task<int> SubmitSurveyAsync(ISurveyResponseModel surveyResponse);
        Task<ISurveyResponseModel> GetExistingResponse(string SectionId, string StoreHistoryUID, DateTime? submmitedDate = null);
        Task GetExistingSummary(string ActivityType, string LinkedItemUID);
        List<IStoreItemView> StoresListByRoute { get; set; }
        Task GetCustomersByRoute(string RouteUID);
        Task<int> UpdateSurveyResponse(ISurveyResponseModel surveyResponseModel);
    }
}
