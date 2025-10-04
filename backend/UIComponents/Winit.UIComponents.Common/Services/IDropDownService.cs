using Microsoft.AspNetCore.Components;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Winit.Shared.Models.Common;
using Winit.Shared.Models.Enums;
using Winit.Shared.Models.Events;

namespace Winit.UIComponents.Common.Services
{
    public interface IDropDownService
    {
        event Action<DropDownOptions> OnShowDropDown;
        event Func<DropDownOptions, Task> OnShowMobilePopUpDropDown;
        public Task ShowDropDown(DropDownOptions options);
        public Task ShowMobilePopUpDropDown(DropDownOptions options);
    }

    public class DropDownOptions
    {
        public List<Winit.Shared.Models.Common.ISelectionItem> DataSource { get; set; }
        public SelectionMode SelectionMode { get; set; }
        public string Title { get; set; }
        public bool Disabled { get; set; }
        public bool IsSearchable { get; set; }
        public Func<DropDownEvent,Task> OnSelect { get; set; }
        public bool IsButtonVisible { get; set; }
        public string UniqueUID { get; set; }
        public string OkBtnTxt { get; set; }
        public bool ShowOtherOption { get; set; }
    }
}
