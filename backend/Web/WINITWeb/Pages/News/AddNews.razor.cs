

using Winit.Modules.NewsActivity.BL.Classes;
using Winit.Modules.NewsActivity.Models.Constants;
using Winit.Shared.CommonUtilities.Common;
using Winit.Shared.Models.Common;
using Winit.Shared.Models.Events;
using Winit.UIComponents.Common.FileUploader;
using WinIt.BreadCrum.Classes;

namespace WinIt.Pages.News
{
    public partial class AddNews
    {
        public FileUploader.FileUploaderV1? fileUploader;
        string FilePath = string.Empty;
        Winit.UIModels.Web.Breadcrum.Interfaces.IDataService dataService = new Winit.UIModels.Web.Breadcrum.Classes.DataServiceModel()
        {
            BreadcrumList = []
        };
        bool IsLoad;
        private void SetBreadCrum()
        {
            if (_viewModel.IsNews)
            {
                dataService.HeaderText = _viewModel.IsNew ? "Add News" : "Edit News";
                dataService.BreadcrumList.Add(new Winit.UIModels.Web.Breadcrum.Classes.BreadCrumModel()
                { SlNo = 1, Text = "Maintain Activity(News)", IsClickable = true, URL = "MaintainNewsActivity" });
                dataService.BreadcrumList.Add(new Winit.UIModels.Web.Breadcrum.Classes.BreadCrumModel()
                { SlNo = 2, Text = _viewModel.IsNew ? "Add News" : "Edit News" });
            }
            else if (_viewModel.IsAdvertisement)
            {
                dataService.HeaderText = _viewModel.IsNew ? "Add Advertisement" : "Edit Advertisement";
                dataService.BreadcrumList.Add(new Winit.UIModels.Web.Breadcrum.Classes.BreadCrumModel()
                { SlNo = 1, Text = "Maintain Activity(Advertisement)", IsClickable = true, URL = "MaintainNewsActivity" });
                dataService.BreadcrumList.Add(new Winit.UIModels.Web.Breadcrum.Classes.BreadCrumModel()
                { SlNo = 2, Text = _viewModel.IsNew ? "Add Advertisement" : "Edit Advertisement" });
            }
            else
            {
                dataService.HeaderText = _viewModel.IsNew ? "Add Business Communication" : "Edit Business Communication";
                dataService.BreadcrumList.Add(new Winit.UIModels.Web.Breadcrum.Classes.BreadCrumModel()
                { SlNo = 1, Text = "Maintain Activity(Business Communication)", IsClickable = true, URL = "MaintainNewsActivity" });
                dataService.BreadcrumList.Add(new Winit.UIModels.Web.Breadcrum.Classes.BreadCrumModel()
                { SlNo = 2, Text = _viewModel.IsNew ? "Add Business Communication" : "Edit Business Communication" });
            }
        }
        protected override async Task OnInitializedAsync()
        {
            ShowLoader();
            await _viewModel.PopulateViewModel();

            FilePath = FileSysTemplateControles.GetNewsActivityFolderPath(_viewModel.NewsActivity.UID);
            SetBreadCrum();
            IsLoad = true;
            base.OnInitializedAsync();
            HideLoader();
        }
        void IsValidated(out bool isVal, out string message)
        {
            isVal = true;
            message = string.Empty;
            if (string.IsNullOrEmpty(_viewModel.NewsActivity.Title))
            {
                message = "Title ,";
                isVal = false;
            }
            if (string.IsNullOrEmpty(_viewModel.NewsActivity.Description))
            {
                isVal = false;
                message = "Description ,";
            }
            if (_viewModel.NewsActivity.PublishDate == null || _viewModel.NewsActivity.PublishDate == DateTime.MinValue)
            {
                isVal = false;
                message = "PublishDate ,";
            }
            if (!isVal)
            {
                message = $"Following fields shouldn't be empty {message.Substring(0, message.Length - 1)}";
            }
        }
        private async Task Save()
        {
            IsValidated(out bool isVal, out string message);
            if (!isVal)
            {
                await _alertService.ShowErrorAlert("Alert", message);
                return;
            }
            ShowLoader();
            try
            {
                NewsActivityWebViewModel viewModel = _viewModel as NewsActivityWebViewModel;
                ApiResponse<string> response = viewModel.IsNew ? await viewModel.CreateNewsActivity() : await viewModel.UpdateNewsActivity();
                if (response != null && response.IsSuccess)
                {
                    if (_viewModel.ModifiedFileSysList.Count > 0)
                    {
                        var res = await fileUploader.MoveFiles();
                        var res1 = await _viewModel.UploadFiles(_viewModel.ModifiedFileSysList.Where(p => p.Id <= 0).ToList());
                        if (res != null && res.IsSuccess && res1 != null && res1.IsSuccess)
                        {
                            _tost.Add("Success", "Saved Successfully");
                            _navigationManager.NavigateTo("MaintainNewsActivity");
                        }
                    }
                    else
                    {
                        _tost.Add("Success", "Saved Successfully");
                        _navigationManager.NavigateTo("MaintainNewsActivity");
                    }
                }
            }
            catch (Exception ex)
            {

            }
            finally
            {
                HideLoader();
            }
        }
        void OnFileTypeSelect(DropDownEvent dropDownEvent)
        {
            if (dropDownEvent != null)
            {
                if (dropDownEvent.SelectionItems != null && dropDownEvent.SelectionItems.Count > 0)
                {
                    _viewModel.SelectedFile = dropDownEvent.SelectionItems.FirstOrDefault().Code;
                    if (_viewModel.SelectedFile == NewsActivityConstants.image)
                    {
                        fileUploader.FileExtension = _viewModel.FileExtensions = ".jpg,.jpeg,.png";
                    }
                    else if (_viewModel.SelectedFile == NewsActivityConstants.Video)
                    {
                        fileUploader.FileExtension = _viewModel.FileExtensions = ".mp4";
                    }
                }
                else
                {
                    _viewModel.SelectedFile = string.Empty;
                    fileUploader.FileExtension = _viewModel.FileExtensions = ".jpg,.jpeg,.png";
                }
            }
            StateHasChanged();
        }
    }
}
