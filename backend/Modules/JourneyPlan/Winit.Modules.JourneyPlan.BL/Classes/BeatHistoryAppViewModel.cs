using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Localization;
using Winit.Modules.Common.BL.Interfaces;
using Winit.Modules.JourneyPlan.BL.Interfaces;
using Winit.UIComponents.Common.Language;
using Winit.UIComponents.Common.LanguageResources.Mobile;

namespace Winit.Modules.JourneyPlan.BL.Classes;

public class BeatHistoryAppViewModel : BeatHistoryBaseViewModel
{
    public BeatHistoryAppViewModel(IServiceProvider serviceProvider,
        IConfiguration config, IAppUser appUser, IBeatHistoryBL beatHistoryBL, IStringLocalizer<LanguageKeys> Localizer,
            ILanguageService languageService) : base(serviceProvider, config,
            appUser, beatHistoryBL, Localizer, languageService)
    {

    }
}
