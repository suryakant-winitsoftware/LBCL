using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Winit.Modules.ErrorHandling.BL.Interfaces;
using Winit.Modules.ErrorHandling.DL.Interfaces;
using Winit.Modules.ErrorHandling.Model.Interfaces;

namespace Winit.Modules.ErrorHandling.BL.Classes
{
    public class ErrorHandlerBL : IErrorHandlerBL
    {
        private Dictionary<string, IErrorDetail> _errorDetailDictionary;
        public ErrorHandlerBL()
        {
            _errorDetailDictionary = new Dictionary<string, IErrorDetail>();
        }
        public async Task SetErrorDetailDictionary(Dictionary<string, IErrorDetail> errorDetailDictionary)
        {
            _errorDetailDictionary = errorDetailDictionary;
            await Task.CompletedTask;
        }
        public async Task<IErrorDetail?> GetErrorDetailAsync(string errorCode, string languageCode)
        {
            string key = $"{errorCode}_{languageCode}";
            if (_errorDetailDictionary != null && _errorDetailDictionary.ContainsKey(key))
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
    }
}
