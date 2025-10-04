using Microsoft.AspNetCore.Components;
using Winit.Modules.Common.Model.Classes;
using Winit.Modules.FileSys.Model.Classes;
using Winit.Modules.FileSys.Model.Interfaces;
using Winit.Shared.CommonUtilities.Common;

namespace WinIt.Pages.Demo
{
    public partial class FileUploadDemo
    {
        bool IsInitialized { get; set; }
        string FilePath {  get; set; }  
       List<string> SavedImagePath { get; set; }
        Winit.UIComponents.Common.FileUploader.FileUploader fileUploader { get; set; }
        protected async override Task OnInitializedAsync()
        {
            SavedImagePath = new List<string>();
            FilePath = FileSysTemplateControles.GetDemoFolderPath(Guid.NewGuid().ToString());
            IsInitialized=true;
        }
        private void GetsavedImagePath(List<IFileSys> ImagePath)
        {
            //SavedImagePath.Add(ImagePath);
        }
        private void AfterDeleteImage()
        {

        }
        public async Task MoveFiles()
        {
            await fileUploader.MoveFiles();
        }
    }
}
