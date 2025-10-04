using Microsoft.Identity.Client;
using Winit.Modules.Common.Model.Constants;
using Winit.Modules.FileSys.BL.Classes;
using Winit.Modules.FileSys.Model.Interfaces;
using Winit.Modules.NewsActivity.Models.Interfaces;
using Winit.Shared.Models.Common;

namespace WinIt.Pages.News
{
    public partial class NewsBoard
    {
        protected async override Task OnInitializedAsync()
        {
            ShowLoader();
            await _viewModel.PopulateviewModel(true);
            HideLoader();
            await base.OnInitializedAsync();
        }
        public bool ShowPopUp { get; set; }
        public bool IsImgTag { get; set; }
        public bool IsVideoTag { get; set; }
        public string ImageSrc { get; set; }
        public string VideoSrc { get; set; }
        public string Title { get; set; }
        public void OnImageClick(string Url)
        {
            try
            {
                ShowPopUp = true;
                IsImgTag = true;
                Title = "";
                ImageSrc = Url;
            }
            catch (Exception ex)
            {
                throw;
            }
            finally
            {
                IsVideoTag = false;
                StateHasChanged();
            }
        }
        public void OnVideoClick(string Url)
        {
            try
            {
                ShowPopUp = true;
                IsVideoTag = true;
                Title = "";
                VideoSrc = Url;
            }
            catch (Exception ex)
            {
                throw;
            }
            finally
            {
                IsImgTag = false;
                StateHasChanged();
            }
        }
        private (string, string) GetImageOrVedioAndURL(INewsActivity newsActivity)
        {
            string url = string.Empty;
            if (newsActivity.FilesysList == null || newsActivity.FilesysList.Count == 0)
                return (string.Empty, url);

            IFileSys fileSys = newsActivity.FilesysList.FirstOrDefault();

            url = $"{_appConfigs.ApiDataBaseUrl}{fileSys.RelativePath}/{fileSys.FileName}";
            return (fileSys.FileType, url);
        }
        private string GetImageURL(List<IFileSys> files)
        {
            string url = string.Empty;
            if (files == null || files.Count == 0)
                return url;

            IFileSys fileSys = files.FirstOrDefault(p => p.FileType == FileTypeConstants.Image);

            if (fileSys is null) return url;
            url = $"{_appConfigs.ApiDataBaseUrl}{fileSys?.RelativePath}/{fileSys.FileName}";
            return url;
        }
        async Task OnTabSelect(ISelectionItem selectedItem)
        {
            ShowLoader();
            await _viewModel.OnTabSelect(selectedItem);
            HideLoader();
            StateHasChanged();
        }
        async Task OnPageChange(int pageNumber)
        {
            ShowLoader();
            await _viewModel.OnPageChange(pageNumber);
            HideLoader();
            StateHasChanged();
        }
    }
}
