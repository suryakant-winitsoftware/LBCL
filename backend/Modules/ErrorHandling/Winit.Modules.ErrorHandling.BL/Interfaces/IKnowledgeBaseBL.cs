using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Winit.Modules.ErrorHandling.Model.Interfaces;
using Winit.Shared.Models.Common;
using Winit.Shared.Models.Enums;

namespace Winit.Modules.ErrorHandling.BL.Interfaces
{
    public interface IKnowledgeBaseBL
    {
        Task<Model.Interfaces.IErrorDetail?> GetErrorDetailAsync(string errorCode, string languageCode);
        Task<Dictionary<string, Model.Interfaces.IErrorDetail>> GetErrorDetailsAsync();
        Task<PagedResponse<Model.Interfaces.IErrorDetailModel>> GetErrorDetails(List<SortCriteria> sortCriterias, int pageNumber,
       int pageSize, List<FilterCriteria> filterCriterias, bool isCountRequired);
        Task<Model.Interfaces.IErrorDetailModel> GetErrorDetailsByUID(string UID);
        Task<int> CreateErrorDetails(IErrorDetailModel errorDetail);
        Task<int> UpdateErrorDetails(IErrorDetailModel errorDetail);
        Task<Model.Interfaces.IErrorDescriptionDetails> GetErrorDescriptionDetailsByErroCode(string errorCode);
        Task<IEnumerable<Model.Interfaces.IErrorDetailsLocalization>> GetErrorDetailsLocalizationByErrorCode(List<string> errorCodeList);
        Task<int> CUDErrorDetailsLocalization(List<Winit.Modules.ErrorHandling.Model.Classes.ErrorDetailsLocalization> errorDetailsLocalizations);
        Task<IErrorDetailsLocalization> GetErrorDetailsLocalizationbyUID(string errorDetailsLocalizationUID);
    }
}
