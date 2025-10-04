using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WINITMobile.State
{
    public interface IBackButtonHandler
    {
        void SetCurrentPage(ICurrentPageHandler page);
        void ClearCurrentPage();
        Task<bool> HandleBackClickAsync();
    }
}
