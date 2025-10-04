using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WINITMobile.State
{
    public class BackButtonHandler : IBackButtonHandler
    {
        private ICurrentPageHandler _currentPage;

        public void SetCurrentPage(ICurrentPageHandler page)
        {
            _currentPage = page;
        }
        public void ClearCurrentPage()
        {
            _currentPage = null;
        }
        public async Task<bool> HandleBackClickAsync()
        {
            if (_currentPage != null)
            {
                await _currentPage.OnBackClick();
                return true;
            }
            return false;
        }
    }
}
