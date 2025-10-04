using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Winit.Modules.Base.BL.Helper.Interfaces;
using Winit.Modules.Common.BL.Interfaces;
using Winit.Modules.Common.Model.Interfaces;
using Winit.Modules.FileSys.Model.Interfaces;
using Winit.Modules.ProductFeedback.BL.Interfaces;
using Winit.Modules.Setting.BL.Interfaces;
using Winit.Modules.SKU.Model.Interfaces;
using Winit.Modules.Store.Model.Interfaces;
using Winit.Shared.Models.Common;

namespace Winit.Modules.ProductFeedback.BL.Classes
{
    public class ProductFeedbackBaseViewModel : IProductFeedbackViewModel
    {
        // Injection
        private IServiceProvider _serviceProvider;
        protected readonly IFilterHelper _filter;
        private readonly ISortHelper _sorter;

        private readonly IListHelper _listHelper;
        protected readonly IAppUser _appUser;
        private readonly IAppSetting _appSetting;
        protected readonly IDataManager _dataManager;
        private readonly IAppConfig _appConfig;
        public List<ISelectionItem> storeItemViews { get; set; }
        public List<ISelectionItem> skuItemViews { get; set; }
        public List<ISKUMaster> SkuMasterItems { get; set; }
        public List<ISKU> SkuItems { get; set; }
        public List<IFileSys> ImageFileSysList { get; set; } = new List<IFileSys>();
        public string FolderPathImages { get; set; }

        public ProductFeedbackBaseViewModel(IServiceProvider serviceProvider,
                IFilterHelper filter,
                ISortHelper sorter,
                IListHelper listHelper,
                IAppUser appUser,
                IAppSetting appSetting,
                IDataManager dataManager,
                IAppConfig appConfig)
        {
            _serviceProvider = serviceProvider;
            _filter = filter;
            _sorter = sorter;

            _listHelper = listHelper;
            _appUser = appUser;
            _appSetting = appSetting;
            _dataManager = dataManager;
            _appConfig = appConfig;
            storeItemViews = new List<ISelectionItem>();
            skuItemViews = new List<ISelectionItem>();
        }

        public virtual async Task PopulateViewModel()
        {
            try
            {
                 
            }
            catch (Exception ex)
            {
                throw;
            }
        }
        
        public virtual async Task SaveFileSys(bool IsSuccess, string ProductFeedbackUID)
        {
            try
            {
                 
            }
            catch (Exception ex)
            {
                throw;
            }
        }
    }
}
