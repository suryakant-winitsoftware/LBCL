using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WINITMobile.Data
{
    public interface ILoadingService
    {
        bool IsLoading { get; }
        public string Message { get; set; }

        event Action OnLoadingStateChanged; 
        void ShowLoading(string message = "");
        void HideLoading();
    }
}
