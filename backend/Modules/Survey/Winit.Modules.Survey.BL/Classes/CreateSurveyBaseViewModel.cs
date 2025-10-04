using Nest;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Winit.Modules.Base.BL.Helper.Interfaces;
using Winit.Modules.Common.BL.Interfaces;
using Winit.Modules.JobPosition.Model.Classes;
using Winit.Modules.JobPosition.Model.Interfaces;
using Winit.Modules.Survey.BL.Interfaces;
using Winit.Modules.Survey.Model.Classes;
using Winit.Modules.Survey.Model.Interfaces;
using Winit.Modules.User.Model.Interfaces;
using Winit.Shared.CommonUtilities.Extensions;
using Winit.Shared.Models.Common;
using Winit.Shared.Models.Enums;
using Winit.UIModels.Common;

namespace Winit.Modules.Survey.BL.Classes
{
    public abstract class CreateSurveyBaseViewModel : ICreateSurveyViewModel
    {
        public List<ManageQuestion> ManageQuestions { get; set; }
        // public ManageSurvey ManageSurveyDTO = new ManageSurvey();
        public ManageSurvey managesurveyModel { get; set; }
        public Winit.Modules.Survey.Model.Classes.Survey surveyModel { get; set; }
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; }
        public int TotalItemsCount { get; set; }
        public List<FilterCriteria> SurveyFilterCriterias { get; set; }
        public List<SortCriteria> SortCriterias { get; set; }

        public List<Winit.Modules.Survey.Model.Classes.Survey> surveyList { get; set; }
        private IServiceProvider _serviceProvider;
        private readonly IFilterHelper _filter;
        private readonly ISortHelper _sorter;
        private readonly IListHelper _listHelper;
        private Winit.Shared.Models.Common.IAppConfig _appConfigs;
        private Winit.Modules.Base.BL.ApiService _apiService;
        IAppUser _appUser;


        public CreateSurveyBaseViewModel(IServiceProvider serviceProvider,
            IFilterHelper filter,
            ISortHelper sorter,
            IListHelper listHelper, Winit.Shared.Models.Common.IAppConfig appConfigs, Winit.Modules.Base.BL.ApiService apiService,
            IAppUser appUser)
        {
            _serviceProvider = serviceProvider;
            _filter = filter;
            _sorter = sorter;
            _listHelper = listHelper;
            _apiService = apiService;
            _appConfigs = appConfigs;
            _appUser = appUser;
            ManageQuestions = new List<ManageQuestion>();
            managesurveyModel = _serviceProvider.CreateInstance<ManageSurvey>();
            surveyModel = _serviceProvider.CreateInstance<Winit.Modules.Survey.Model.Classes.Survey>();
            surveyList = new List<Model.Classes.Survey>();
            SurveyFilterCriterias = new List<FilterCriteria>();
            SortCriterias = new List<SortCriteria>();
        }
       
        public async Task ApplySort(SortCriteria sortCriteria)
        {
            SortCriterias.Clear();
            SortCriterias.Add(sortCriteria);
            await GetSurveyData();
        }
        public virtual async Task GetSurveyData()
        {
            surveyList.Clear();
            surveyList.AddRange(await GetSurveyDetails() ?? new());
        }
        public async Task PageIndexChanged(int pageNumber)
        {
            PageNumber = pageNumber;
            await GetSurveyData();
        }
        public string GetDateOnlyInFormat(string value)
        {
            try
            {
                string dateValueString = value;
                DateTime dateValue;

                if (DateTime.TryParse(dateValueString, out dateValue))
                {
                    return dateValue.ToString("yyyy-MM-dd");
                    // Use the formattedDate as needed
                }
                return value;
            }
            catch (Exception ex)
            {
                throw;
            }
        }
        public async Task OnFilterApply(Dictionary<string, string> keyValuePairs)
        {
            try
            {
                SurveyFilterCriterias.Clear();
                foreach (var keyValue in keyValuePairs)
                {
                    if (!string.IsNullOrEmpty(keyValue.Value))
                    {
                        if (keyValue.Value.Contains(","))
                        {
                            string[] values = keyValue.Value.Split(',');
                            SurveyFilterCriterias.Add(new FilterCriteria(keyValue.Key, values, FilterType.In));
                        }
                        else if (keyValue.Key.Contains("Date"))
                        {
                            if (keyValue.Key == "StartDate")
                                SurveyFilterCriterias.Add(new FilterCriteria(keyValue.Key, GetDateOnlyInFormat(keyValue.Value), FilterType.GreaterThanOrEqual));
                            if (keyValue.Key == "EndDate")
                                SurveyFilterCriterias.Add(new FilterCriteria(keyValue.Key, GetDateOnlyInFormat(keyValue.Value), FilterType.LessThanOrEqual));
                        }
                        else if (keyValue.Key == "IsActive")
                        {
                            bool transformedValue = keyValue.Value == "Active" ? true :
                             keyValue.Value == "InActive" ? false :
                             Convert.ToBoolean(keyValue.Value);


                            SurveyFilterCriterias.Add(new FilterCriteria(@$"{keyValue.Key}", transformedValue, FilterType.Equal));
                        }


                        else
                        {
                            SurveyFilterCriterias.Add(new FilterCriteria(keyValue.Key, keyValue.Value, FilterType.Like));
                        }
                    }
                }
                PageNumber = 1;
                await GetSurveyData();
            }
            catch (Exception ex)
            {

            }
        }
        public async Task<Winit.Modules.Survey.Model.Classes.ManageSurvey> PopulateSurveyDetailsforEdit(string Uid)
        {
            var surveyModel = await GetSurveyDetailsforEdit(Uid);
            if (surveyModel == null) return null;

            // Map the Survey object to ManageSurvey
            return GetMappedSurvey(surveyModel);
        }
        public ManageSurvey GetMappedSurvey(ISurvey surveyModel)
        {
            if (!string.IsNullOrEmpty(surveyModel.SurveyData))
            {
                try
                {
                    // Deserialize JSON into ManageSurvey
                    var deserializedSurvey = JsonConvert.DeserializeObject<ManageSurvey>(surveyModel.SurveyData);
                    // Assign deserialized values
                    managesurveyModel = deserializedSurvey;
                    return managesurveyModel;

                }
                catch (JsonException ex)
                {
                    Console.WriteLine($"JSON Deserialization Error: {ex.Message}");
                }
            }



            return null;
        }
        public bool IsNew { get; set; }
      
        public async Task<bool> SaveOrUpdateSurveyData(ManageSurvey manageSurvey)
        {
            try
            {
                // Fetch the existing survey from the session or database
                var existingSurvey = await GetSurveyDetailsByCode(manageSurvey.Code);
                // If the survey exists, update the existing data
                if (existingSurvey != null)
                {
                    
                    IsNew = false;
                    // Deserialize the existing SurveyData
                    ManageSurvey existingManageSurvey = JsonConvert.DeserializeObject<ManageSurvey>(existingSurvey.SurveyData);
                    // Update the survey details
                    existingManageSurvey.Title = manageSurvey.Title;
                    existingManageSurvey.Description = manageSurvey.Description;
                    existingManageSurvey.IsActive = manageSurvey.IsActive;
                    existingManageSurvey.StartDate = manageSurvey.StartDate;
                    existingManageSurvey.EndDate = manageSurvey.EndDate;
                    existingManageSurvey.Sections = manageSurvey.Sections;
                   
                    // Iterate over the sections and update or add as needed
                    int sectionIndex = 1;
                    foreach (var newSection in existingManageSurvey.Sections)
                    {
                        // Find if the section already exists based on SectionTitle
                        var existingSection = existingManageSurvey.Sections
                            .FirstOrDefault(s => s.SectionTitle == newSection.SectionTitle);

                        if (existingSection != null)
                        {
                            existingSection.SectionId = string.IsNullOrEmpty(existingSection.SectionId)
                      ? Guid.NewGuid().ToString()
                      : existingSection.SectionId;

                            existingSection.SectionTitle = !string.IsNullOrEmpty(newSection.SectionTitle)
                                ? newSection.SectionTitle
                                : existingSection.SectionTitle;

                            existingSection.SeqNo = sectionIndex++;
                            // Update questions in the existing section
                            int questionIndex = 1;
                            foreach (var newQuestion in newSection.Questions)
                            {
                                var existingQuestion = existingSection.Questions
                                    .FirstOrDefault(q => q.Id == newQuestion.Id);

                                if (existingQuestion != null)
                                {
                                    // Update existing question
                                    existingQuestion.LabelQuestion = newQuestion.LabelQuestion;
                                    existingQuestion.Type = newQuestion.Type;
                                    existingQuestion.Label = newQuestion.Label;
                                    existingQuestion.IsScoreRequired = newQuestion.IsScoreRequired;
                                    existingQuestion.IsCameraVisible = newQuestion.IsCameraVisible;
                                    existingQuestion.IsRequired = newQuestion.IsRequired;
                                    existingQuestion.MinDate = newQuestion.MinDate;
                                    existingQuestion.MaxDate = newQuestion.MaxDate;
                                    existingQuestion.MinSpecificDate = newQuestion.MinSpecificDate;
                                    existingQuestion.MaxSpecificDate = newQuestion.MaxSpecificDate;
                                    existingQuestion.IsDateRequired = newQuestion.IsDateRequired;
                                    existingQuestion.IsTimeRequired = newQuestion.IsTimeRequired;
                                    existingQuestion.Options = newQuestion.Options;
                                    existingQuestion.SeqNo = questionIndex++; // Ensure question SeqNo is updated
                                    existingQuestion.Validations.is_mandatory = newQuestion.Validations.is_mandatory;
                                }
                                else
                                {
                                    // Add the new question if it doesn't exist
                                    newQuestion.SeqNo = questionIndex++; // Assign new sequence number
                                    existingSection.Questions.Add(newQuestion);
                                }
                            }
                        }
                        else
                        {
                            // Add new section if it doesn't exist
                            newSection.SeqNo = sectionIndex++; // Assign new sequence number
                            existingManageSurvey.Sections.Add(newSection);
                        }
                    }
                    existingSurvey.Description = existingManageSurvey.Description;
                    existingSurvey.IsActive = existingManageSurvey.IsActive;
                    // Convert the updated survey back to JSON
                    existingSurvey.SurveyData = JsonConvert.SerializeObject(existingManageSurvey, Formatting.Indented);
                    
                    // Save the updated survey data
                    int result = await CreateSurvey(existingSurvey, false);  // Pass false for update
                    return result > 0;
                }
                else
                {
                   
                    IsNew = true;
                    // If survey does not exist, create a new one
                    var newSurvey = new Model.Classes.Survey
                    {
                        UID = manageSurvey.Code,
                        Description = manageSurvey.Description,
                        IsActive = manageSurvey.IsActive,
                        CreatedBy = _appUser.Emp.UID,
                        CreatedTime = DateTime.Now,
                        ModifiedBy=_appUser.Emp.UID,
                        ModifiedTime = DateTime.Now,
                        ServerModifiedTime= DateTime.Now,
                        ServerAddTime = DateTime.Now,
                        SS = 0,
                        Code = manageSurvey.Code,
                        StartDate = !string.IsNullOrWhiteSpace(manageSurvey.StartDate) && DateTime.TryParse(manageSurvey.StartDate, out DateTime startDate) ? startDate : (DateTime?)null,
                        EndDate = !string.IsNullOrWhiteSpace(manageSurvey.EndDate) && DateTime.TryParse(manageSurvey.EndDate, out DateTime endDate) ? endDate : (DateTime?)null,

                        SurveyData = JsonConvert.SerializeObject(new ManageSurvey
                        {
                            SurveyId = Guid.NewGuid().ToString(),
                            Title = manageSurvey.Title,
                            Code = manageSurvey.Code,
                            Description = manageSurvey.Description,
                            IsActive = manageSurvey.IsActive,
                            StartDate = manageSurvey.StartDate,
                            EndDate = manageSurvey.EndDate,
                            Sections = manageSurvey.Sections.Select((section, sectionIndex) => new ManageSection
                            {
                                SectionId = Guid.NewGuid().ToString(),
                                SectionTitle = manageSurvey.Title,
                                SeqNo = sectionIndex + 1,
                                Questions = section.Questions.Select((q, questionIndex) => new ManageQuestion
                                {
                                    Id = q.Id,
                                    LabelQuestion = q.LabelQuestion,
                                    Type = q.Type,
                                    Label = q.Label,
                                    Validations = q.Validations,
                                    SeqNo = questionIndex + 1,
                                    IsScoreRequired = q.IsScoreRequired,
                                    IsCameraVisible = q.IsCameraVisible,
                                    IsRequired = q.IsRequired,
                                    MinDate = q.MinDate,
                                    MaxDate = q.MaxDate,
                                    MinSpecificDate=q.MinSpecificDate,
                                    MaxSpecificDate=q.MaxSpecificDate,
                                    IsDateRequired = q.Type == CreateSurveyConstants.DropDownTypes.DateTime && q.IsDateRequired,
                                    IsTimeRequired = q.Type == CreateSurveyConstants.DropDownTypes.DateTime && q.IsTimeRequired,
                                    Options = q.Type == CreateSurveyConstants.DropDownTypes.DateTime
                                        ? null
                                        : q.Options?.Select(opt => new ManageOption
                                        {
                                            Label = opt.Label,
                                            LabelOption = opt.LabelOption,
                                            Points = opt.Points
                                        }).ToList(),

                                }).ToList(),
                            }).ToList()
                        }, Formatting.Indented)
                    };

                    // Save the new survey
                    int saveResult = await CreateSurvey(newSurvey, true);  // Create new survey
                    return saveResult > 0;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in SaveOrUpdateSurveyData: {ex.Message}");
                return false;
            }
        }


        public abstract Task<int> CreateSurvey(ISurvey survey, bool Iscreate);
        public abstract Task<List<Model.Classes.Survey>> GetSurveyDetails();
        public abstract Task<Model.Interfaces.ISurvey> GetSurveyDetailsforEdit(string uid);
        public abstract Task<Model.Interfaces.ISurvey> GetSurveyDetailsByCode(string code);

    }
}
