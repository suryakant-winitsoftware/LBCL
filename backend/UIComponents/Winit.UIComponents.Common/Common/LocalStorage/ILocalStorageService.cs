using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Winit.UIComponents.Common.Common.LocalStorage
{
    public interface ILocalStorageService
    {
        Task SetItem<T>(string key, T value);
        Task<T> GetItem<T>(string key);
    }
}
