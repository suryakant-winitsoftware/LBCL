using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using System;
using System.Collections.Generic;
using System.Linq;
using Winit.Modules.FileSys.Model.Interfaces;

namespace Winit.UIComponents.Common.FileUploader
{
    public partial class ImageViewer : ComponentBase
    {
        bool ShowImageInPopup { get; set; }
        string PopUpImagePath { get; set; }
        [Parameter]public bool IsDeleteRequired { get; set; }
        [Parameter]public List<IFileSys> FileSysList { get; set; }
        [Parameter]public EventCallback<Modules.FileSys.Model.Classes.FileSys> OnImageDelete {  get; set; }
        List<IFileSys> Images = new List<IFileSys>();
        protected override void OnInitialized()
        {
            if (FileSysList != null)
            {
                Images = FileSysList;
            }
            base.OnInitialized();
        }
        private async Task OnImageOrPdfClick(Modules.FileSys.Model.Classes.FileSys fileSys)
        {
            PopUpImagePath = fileSys.Id == 0 ? $"{_appConfig.ApiDataBaseUrl}{fileSys.TempPath}/{fileSys.FileName}" : $"{_appConfig.ApiDataBaseUrl}{fileSys.RelativePath}/{fileSys.FileName}";
            //if (fileSys.FileSysFileType == Modules.FileSys.Model.Classes.FileType.Image)
            //{
                ShowImageInPopup = true;
            //}


        }
        private async Task Delete(Modules.FileSys.Model.Classes.FileSys fileSys)
        {
            if(await _alerService.ShowConfirmationReturnType("Alert", "Are you sure you want to Delete?"))
            {
                await OnImageDelete.InvokeAsync(fileSys);
            }
        }
        protected Winit.Modules.FileSys.Model.Classes.FileType IsImageOrPDF(string fileName, string filextn = null)
        {
            Winit.Modules.FileSys.Model.Classes.FileType fileType;
            if (!string.IsNullOrEmpty(fileName))
            {
                if (filextn == null)
                {
                    filextn = Path.GetExtension(fileName);
                }
                if (filextn == ".pdf")
                {
                    return Winit.Modules.FileSys.Model.Classes.FileType.Pdf;
                }
                else if (filextn == ".jpg" || filextn == ".jpeg" || filextn == ".png")
                {
                    return Winit.Modules.FileSys.Model.Classes.FileType.Image;
                }
            }
            return Winit.Modules.FileSys.Model.Classes.FileType.None;

        }

    }
}
