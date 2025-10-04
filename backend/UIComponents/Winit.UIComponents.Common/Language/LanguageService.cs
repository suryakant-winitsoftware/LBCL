using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Winit.UIComponents.Common.Language
{
    public class LanguageService : ILanguageService
    {
        private string _selectedCulture = "ar"; 

        public string SelectedCulture
        {
            get => _selectedCulture;
            set
            {
                _selectedCulture = value;
                OnLanguageChanged?.Invoke(this, value);

            }
        }

        public event EventHandler<string> OnLanguageChanged;
    }
}
