using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Winit.Modules.Base.BL;
using Winit.Modules.Base.Model;
using Winit.Modules.Common.BL.Interfaces;
using Winit.Modules.Common.Model.Interfaces;
using Winit.Modules.FileSys.BL.Classes;
using Winit.Modules.FileSys.BL.Interfaces;
using Winit.Modules.FileSys.Model.Interfaces;
using Winit.Modules.NewsActivity.BL.Interfaces;
using Winit.Modules.NewsActivity.Models.Constants;
using Winit.Modules.NewsActivity.Models.Interfaces;
using Winit.Shared.CommonUtilities.Common;
using Winit.Shared.Models.Common;
using Winit.Shared.Models.Constants;

namespace Winit.Modules.NewsActivity.BL.Classes
{
    public class NewsActivityBaseViewModel : FilesysWebviewModel, INewsActivityViewModel
    {
        public List<ISelectionItem> FileTypes { get; set; } = [];
        public string SelectedFile { get; set; } = string.Empty;
        protected IDataManager? _dataManager { get; set; }
        protected IAppConfig _appConfig { get; set; }
        protected CommonFunctions _commonFunctions { get; set; }
        protected IAppUser _appUser { get; set; }
        public bool IsNews { get; set; }
        public bool IsAdvertisement { get; set; }
        public bool IsBusinessCommunication { get; set; }
        public NewsActivityBaseViewModel(IAppConfig appConfig, ApiService apiService) : base(appConfig, apiService)
        {
        }
        public INewsActivity NewsActivity { get; set; }
        public bool IsNew { get; set; }

        public async Task PopulateViewModel()
        {
            IsNew = PageType.New.Equals(_commonFunctions.GetParameterValueFromURL(PageType.Page), StringComparison.OrdinalIgnoreCase);
            if (IsNew)
            {
                CreateFields(NewsActivity);
                NewsActivity!.ActivityType = _commonFunctions.GetParameterValueFromURL(PageType.Type);
            }
            else
            {
                string uid = _commonFunctions.GetParameterValueFromURL("UID");
                await GetNewsActivityBYUID(uid);
                FileSysList = await GetFilesFilesByLinkedItemUID(uid);
            }
            SetPage();
        }
        protected void SetPage()
        {
            FileTypes.Clear();
            FileTypes.Add(new SelectionItem() { Code = NewsActivityConstants.image, Label = "Image", IsSelected = true });
            SelectedFile = NewsActivityConstants.image;
            switch (NewsActivity!.ActivityType)
            {
                case NewsActivityConstants.news:
                    IsNews = true;
                    break;
                case NewsActivityConstants.advertisement:
                    SelectedFile = string.Empty;
                    FileTypes.Add(new SelectionItem() { Code = NewsActivityConstants.Video, Label = "Video" });
                    IsAdvertisement = true;
                    break;
                default:
                    IsBusinessCommunication = true;
                    break;

            }
        }
        protected void CreateFields(IBaseModel baseModel)
        {
            baseModel.UID = Guid.NewGuid().ToString();
            baseModel.CreatedBy = _appUser.Emp.UID;
            baseModel.ModifiedBy = _appUser.Emp.UID;
            baseModel.CreatedTime = DateTime.Now.Date;
            baseModel.ModifiedTime = DateTime.Now.Date;

        }

        public virtual async Task GetNewsActivityBYUID(string uid)
        {

        }
        public virtual async Task GetNewsActivityUID(string uid)
        {

        }

    }
}
