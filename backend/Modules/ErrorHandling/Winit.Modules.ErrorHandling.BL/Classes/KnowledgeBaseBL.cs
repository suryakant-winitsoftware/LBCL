using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Winit.Modules.ErrorHandling.DL.Interfaces;
using Winit.Modules.ErrorHandling.Model.Interfaces;
using Winit.Shared.Models.Common;
using Winit.Shared.Models.Enums;

namespace Winit.Modules.ErrorHandling.BL.Classes
{
    public class KnowledgeBaseBL : Interfaces.IKnowledgeBaseBL
    {
        private readonly IKnowledgeBaseDL _knowledgeBaseDL;
        private readonly Dictionary<string, IErrorDetail> _errorDetailDictionary;
        public KnowledgeBaseBL(IKnowledgeBaseDL knowledgeBaseDL)
        {
            _knowledgeBaseDL = knowledgeBaseDL;
            _errorDetailDictionary = new Dictionary<string, IErrorDetail>();
        }
        public async Task<IErrorDetail?> GetErrorDetailAsync(string errorCode, string languageCode)
        {
            string key = $"{errorCode}_{languageCode}";
            if (_errorDetailDictionary.ContainsKey(key))
            {
                return _errorDetailDictionary[key];
            }
            else
            {
                // Error detail not found in the dictionary
                await Task.CompletedTask;
                return null;
            }
        }

        public async Task<Dictionary<string, IErrorDetail>> GetErrorDetailsAsync()
        {
            Dictionary<string, IErrorDetail> errorDetailDictionary = new Dictionary<string, IErrorDetail>();
            List<IErrorDetail> errorDetails = await _knowledgeBaseDL.GetErrorDetailsAsync();
            if(errorDetails != null && errorDetails.Count > 0)
            {
                // Populate the dictionary with error details
                foreach (var errorDetail in errorDetails)
                {
                    string key = $"{errorDetail.ErrorCode}_{errorDetail.LanguageCode}";
                    if (!_errorDetailDictionary.ContainsKey(key))
                    {
                        _errorDetailDictionary.Add(key, errorDetail);
                    }
                }
                errorDetailDictionary = errorDetails.ToDictionary(e=>e.ErrorCode + e.LanguageCode, e=>e);
            }
            return errorDetailDictionary;
        }
        public async Task<PagedResponse<Model.Interfaces.IErrorDetailModel>> GetErrorDetails(List<SortCriteria> sortCriterias, int pageNumber,
       int pageSize, List<FilterCriteria> filterCriterias, bool isCountRequired)
        {
            return await _knowledgeBaseDL.GetErrorDetails(sortCriterias, pageNumber, pageSize, filterCriterias, isCountRequired);
        }
        public async Task<Model.Interfaces.IErrorDetailModel> GetErrorDetailsByUID(string UID)
        {
            return await _knowledgeBaseDL.GetErrorDetailsByUID(UID);
        }
        public async Task<int> CreateErrorDetails(IErrorDetailModel errorDetail)
        {
            return await _knowledgeBaseDL.CreateErrorDetails(errorDetail);
        }
        public async Task<int> UpdateErrorDetails(IErrorDetailModel errorDetail)
        {
            return await _knowledgeBaseDL.UpdateErrorDetails(errorDetail);
        }
        public async Task<Model.Interfaces.IErrorDescriptionDetails> GetErrorDescriptionDetailsByErroCode(string errorCode)
        {
            return await _knowledgeBaseDL.GetErrorDescriptionDetailsByErroCode(errorCode);
        }
        public async Task<IEnumerable<Model.Interfaces.IErrorDetailsLocalization>> GetErrorDetailsLocalizationByErrorCode(List<string> errorCodeList)
        {
            return await _knowledgeBaseDL.GetErrorDetailsLocalizationByErrorCode(errorCodeList);
        }
        public async Task<int> CUDErrorDetailsLocalization(List<Winit.Modules.ErrorHandling.Model.Classes.ErrorDetailsLocalization> errorDetailsLocalizations)
        {
            return await _knowledgeBaseDL.CUDErrorDetailsLocalization(errorDetailsLocalizations);
        }
        public async Task<IErrorDetailsLocalization> GetErrorDetailsLocalizationbyUID(string errorDetailsLocalizationUID)
        {
            return await _knowledgeBaseDL.GetErrorDetailsLocalizationbyUID(errorDetailsLocalizationUID);
        }
    }
}
