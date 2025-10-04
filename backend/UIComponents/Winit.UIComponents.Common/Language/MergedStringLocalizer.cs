using Microsoft.Extensions.Localization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Winit.UIComponents.Common.Language;

public class MergedStringLocalizer : IStringLocalizer
{
    private readonly IStringLocalizer _localLocalizer;
    private readonly IStringLocalizer _commonLocalizer;

    public MergedStringLocalizer(IStringLocalizer dashBoardLocalizer, IStringLocalizer commonLocalizer)
    {
        _localLocalizer = dashBoardLocalizer;
        _commonLocalizer = commonLocalizer;
    }

    public LocalizedString this[string name] => GetLocalizedString(name);

    public LocalizedString this[string name, params object[] arguments] => GetLocalizedString(name, arguments);

    public IEnumerable<LocalizedString> GetAllStrings(bool includeParentCultures) => throw new NotImplementedException();

    private LocalizedString GetLocalizedString(string name, params object[] arguments)
    {
        var dashBoardString = _localLocalizer[name];
        if (!dashBoardString.ResourceNotFound)
            return dashBoardString;

        var commonString = _commonLocalizer[name];
        return commonString;
    }
}
