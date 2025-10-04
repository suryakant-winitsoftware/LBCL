using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Winit.Modules.FileSys.Model.Interfaces;
using Winit.Shared.Models.Common;

namespace Winit.Modules.FileSys.BL.Interfaces
{
    public interface IFilesysViewModel
    {
        List<IFileSys> FileSysList { get; set; }
        List<IFileSys> ModifiedFileSysList { get; set; }
        void OnFilesUpload(List<IFileSys> fileSys);
        string FileExtensions {  get; set; }
        Task<ApiResponse<string>> UploadFiles(List<Winit.Modules.FileSys.Model.Interfaces.IFileSys> files);
    }
}
