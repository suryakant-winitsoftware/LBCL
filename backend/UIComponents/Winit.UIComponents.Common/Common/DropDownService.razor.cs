using Microsoft.AspNetCore.Components;

namespace Winit.UIComponents.Common.Common
{
    public partial class DropDown
    {
        [Inject]
        Winit.UIComponents.Common.Services.IDropDownService ?_dropdownService { get; set; }
    }
}
