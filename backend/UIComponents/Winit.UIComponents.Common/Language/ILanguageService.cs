using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Winit.UIComponents.Common.Language
{
    public interface ILanguageService
    {
        string SelectedCulture { get; set; }
        event EventHandler<string> OnLanguageChanged;
    }
}
