using Microsoft.AspNetCore.Components.Forms;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Winit.Shared.Models.Common;

namespace Winit.UIComponents.Common.FileUploader
{
    public interface IFileUploaderBaseViewModel
    {
        List<string> SavedImageList { get; set; }
        List<Winit.Modules.FileSys.Model.Interfaces.IFileSys> AllImages { get; set; }
        IBrowserFile SelectedFile { get; set; }
        Task<bool> DeleteFile(Modules.FileSys.Model.Classes.FileSys fileSys);
        Task<bool> UploadFile(string linkedItemType, string linkedItemUID, string fileType, string fileSysType, string relativePath, bool isDirectory);
        Task<ApiResponse<string>> MoveFiles(string relativePath);
        Task HandleFileSelected(InputFileChangeEventArgs e);
        void PopulateViewModel(string filePath, string FileExtension, long FileSize, string ErrorMessage);
        void PopulateWhenParameterChanged();
    }
}
