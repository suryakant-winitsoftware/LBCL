using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Winit.Modules.Base.BL;
using Winit.Modules.Base.BL.Helper.Interfaces;
using Winit.Modules.Common.BL.Interfaces;
using Winit.Modules.Common.Model.Interfaces;
using Winit.Modules.FileSys.Model.Interfaces;
using Winit.Modules.Setting.BL.Interfaces;
using Winit.Modules.Store.Model.Interfaces;
using Winit.Modules.Survey.BL.Interfaces;
using Winit.Modules.Survey.Model.Interfaces;
using Winit.Shared.Models.Common;

namespace Winit.Modules.Survey.BL.Classes
{
    public abstract class SurveyBaseViewModel : ISurveyViewModel
    {
        public string FolderPathVidoes { get; set; }
        public string FolderPathImages { get; set; }
        public ISurveySection SurveySection { get; set; }
        public Dictionary<string, ISurveyResponse> Responses { get; set; } = new();
        public List<ISurveyResponseModel> SurveyResponseModels { get; set; } = new();
        public List<IStoreItemView> StoresListByRoute { get; set; } 
        public List<IServeyQuestions> Questions { get; set; }
        public List<IFileSys> ImageFileSysList { get; set; } = new List<IFileSys>();
        // Injection
        private IServiceProvider _serviceProvider;
        protected readonly IFilterHelper _filter;
        private readonly ISortHelper _sorter;

        private readonly IListHelper _listHelper;
        protected readonly IAppUser _appUser;
        private readonly IAppSetting _appSetting;
        protected readonly IDataManager _dataManager;
        public readonly IAppConfig _appConfig;
        public ApiService _apiService { get; set; }


        //Constructor
        public SurveyBaseViewModel(IServiceProvider serviceProvider,
                IFilterHelper filter,
                ISortHelper sorter,
                IListHelper listHelper,
                IAppUser appUser,
                IAppSetting appSetting,
                IDataManager dataManager,
                IAppConfig appConfig,ApiService apiService
                )
        {
            _serviceProvider = serviceProvider;
            _filter = filter;
            _sorter = sorter;

            _listHelper = listHelper;
            _appUser = appUser;
            _appSetting = appSetting;
            _dataManager = dataManager;
            _appConfig = appConfig;
            _apiService = apiService;
            StoresListByRoute = new List<IStoreItemView>();
        }

        public virtual async Task GetSurveySection(string UID)
        {

        }

        public virtual async Task<int> SubmitSurveyAsync(ISurveyResponseModel surveyResponse)
        {
            return 0;
        }

        public virtual async Task<ISurveyResponseModel> GetExistingResponse(string SectionId, string StoreHistoryUID, DateTime? submmitedDate)
        {
            return null;
        }

        public virtual async Task GetExistingSummary(string ActivityType, string LinkedItemUID)
        {

        }

        public virtual async Task GetCustomersByRoute(string RouteUID)
        {
           
        }

        public virtual async Task<int> UpdateSurveyResponse(ISurveyResponseModel surveyResponseModel)
        {
            return 0;
        }
    }
}
