using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Winit.Modules.Common.UIState.Classes
{
    public class NavigationHistoryService
    {
        private string _previousUrl;
        private string _currentUrl;

        public string PreviousUrl => _previousUrl;
        public string CurrentUrl => _currentUrl;

        public void RecordNavigation(string newUrl)
        {
            _previousUrl = _currentUrl;
            _currentUrl = newUrl;
        }
    }
}
