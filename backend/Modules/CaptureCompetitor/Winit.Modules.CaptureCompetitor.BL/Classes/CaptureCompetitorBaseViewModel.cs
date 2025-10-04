using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Winit.Modules.Base.BL.Helper.Interfaces;
using Winit.Modules.CaptureCompetitor.BL.Interfaces;
using Winit.Modules.CaptureCompetitor.Model.Interfaces;
using Winit.Modules.Common.BL.Interfaces;
using Winit.Modules.Common.Model.Interfaces;
using Winit.Modules.FileSys.Model.Interfaces;
using Winit.Modules.Setting.BL.Interfaces;
using Winit.Shared.Models.Common;

namespace Winit.Modules.CaptureCompetitor.BL.Classes
{
    public abstract class CaptureCompetitorBaseViewModel : ICaptureCompetitorViewModel
    {
        public List<ISelectionItem> Brands { get; set; }
        public ICaptureCompetitor SelectedcaptureCompetitor { get; set; }
        public ICaptureCompetitor CreateCaptureCompetitor { get; set; }
        public List<ICaptureCompetitor> ListOfCaptureCompetitors { get; set; }
        public List<IFileSys> ImageFileSysList { get; set; } = new List<IFileSys>();

        // Injection
        private IServiceProvider _serviceProvider;
        protected readonly IFilterHelper _filter;
        private readonly ISortHelper _sorter;

        private readonly IListHelper _listHelper;
        protected readonly IAppUser _appUser;
        private readonly IAppSetting _appSetting;
        protected readonly IDataManager _dataManager;
        private readonly IAppConfig _appConfig;



        //Constructor
        public CaptureCompetitorBaseViewModel(IServiceProvider serviceProvider,
                IFilterHelper filter,
                ISortHelper sorter,
                IListHelper listHelper,
                IAppUser appUser,
                IAppSetting appSetting,
                IDataManager dataManager,
                IAppConfig appConfig
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
            Brands = new List<ISelectionItem>();
            SelectedcaptureCompetitor = new Winit.Modules.CaptureCompetitor.Model.Classes.CaptureCompetitor(); 
            CreateCaptureCompetitor = new Winit.Modules.CaptureCompetitor.Model.Classes.CaptureCompetitor();
        }

        public async Task PopulateViewModel()
        {

            await GetTheBrands();
            await GetAllCapatureCampitators();
            await Task.CompletedTask;
        }

        public virtual async Task GetAllCapatureCampitators()
        {

        }
        public virtual async Task GetTheBrands()
        {

        }
        public virtual async Task<int> SaveCompitator()
        {
            return 0;
        }
 
    }
}
