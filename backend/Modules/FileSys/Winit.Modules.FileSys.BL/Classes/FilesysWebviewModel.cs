using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Winit.Modules.Base.BL;
using Winit.Modules.Common.BL.Interfaces;
using Winit.Modules.FileSys.BL.Interfaces;
using Winit.Modules.FileSys.Model.Interfaces;
using Winit.Shared.Models.Common;

namespace Winit.Modules.FileSys.BL.Classes
{
    public class FilesysWebviewModel : IFilesysViewModel
    {
        ApiService _apiService { get; }
        IAppConfig _appConfig { get; }

        public FilesysWebviewModel(IAppConfig appConfig, ApiService apiService)
        {
            _apiService = apiService;
            _appConfig = appConfig;
        }
        public string FileExtensions { get; set; } = ".jpg,.jpeg,.png";
        public List<IFileSys> FileSysList { get; set; } = [];
        public List<IFileSys> ModifiedFileSysList { get; set; } = [];
        public void OnFilesUpload(List<IFileSys> fileSys)
        {
            if (fileSys != null)
                ModifiedFileSysList.AddRange(fileSys.Except(ModifiedFileSysList));
        }
        public async Task<ApiResponse<string>> UploadFiles(List<Winit.Modules.FileSys.Model.Interfaces.IFileSys> files)
        {
            return await _apiService.FetchDataAsync(
            $"{_appConfig.ApiBaseUrl}FileSys/CreateFileSysForBulk", HttpMethod.Post, files);
        }
        protected async Task<List<IFileSys>> GetFilesFilesByLinkedItemUID(string linkedItemUID)
        {
            ApiResponse<List<IFileSys>> apiResponse = await _apiService.FetchDataAsync<List<IFileSys>>(
            $"{_appConfig.ApiBaseUrl}FileSys/GetFileSysByLinkeditemUID?LinkeditemUID={linkedItemUID}", HttpMethod.Get);

            return apiResponse != null && apiResponse.Data != null ? apiResponse.Data : [];
        }
    }
}
