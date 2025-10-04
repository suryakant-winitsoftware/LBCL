using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Winit.Modules.Survey.Model.Classes;
using Winit.Shared.Models.Enums;

namespace Winit.Modules.Survey.BL.Interfaces
{
    public interface ICreateSurveyViewModel
    {
        public ManageSurvey managesurveyModel { get; set; }
        public Winit.Modules.Survey.Model.Classes.Survey surveyModel { get; set; }
        public List<Winit.Modules.Survey.Model.Classes.Survey> surveyList { get; set; }
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
        public int TotalItemsCount { get; set; }
        //  Task ApplyFilter(List<Shared.Models.Enums.FilterCriteria> filterCriterias);
        Task OnFilterApply(Dictionary<string, string> keyValuePairs);
        Task ApplySort(SortCriteria sortCriteria);

        Task PageIndexChanged(int pageNumber);
        public bool IsNew { get; set; }
        public List<ManageQuestion> ManageQuestions { get; set; }
        // Task<bool> SaveSurveyData(ManageSurvey manageSurvey, bool Iscreate);
        //Task<bool> GetManageSurveyJsonData(ManageSurvey survey);
        Task GetSurveyData();
        Task<Winit.Modules.Survey.Model.Classes.ManageSurvey> PopulateSurveyDetailsforEdit(string Uid);
        ManageSurvey GetMappedSurvey(Winit.Modules.Survey.Model.Interfaces.ISurvey surveyModel);
        Task<bool> SaveOrUpdateSurveyData(ManageSurvey manageSurvey);
    }
}
