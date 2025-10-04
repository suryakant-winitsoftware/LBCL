using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Winit.Modules.ErrorHandling.Model.Interfaces;

namespace Winit.Modules.ErrorHandling.BL.Interfaces
{
    public interface IErrorHandlerBL
    {
        Task SetErrorDetailDictionary(Dictionary<string, IErrorDetail> errorDetailDictionary);
        Task<IErrorDetail?> GetErrorDetailAsync(string errorCode, string languageCode);
    }
}
