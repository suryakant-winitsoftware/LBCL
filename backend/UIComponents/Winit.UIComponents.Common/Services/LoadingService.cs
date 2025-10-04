using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Winit.UIComponents.Common.Services
{
    public class LoadingService : ILoadingService
    {
        public bool IsLoading { get; private set; }
        public string Message { get; set; }

        public event Action OnLoadingStateChanged;

        public void ShowLoading(string message = "Loading")
        {
            IsLoading = true;
            Message = message;
            OnLoadingStateChanged?.Invoke();
        }

        public void HideLoading()
        {
            IsLoading = false;
            Message = "Loading";
            OnLoadingStateChanged?.Invoke();
        }
    }
}
